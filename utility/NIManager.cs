using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNI;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading;

namespace Com.Imola.Retina.Utility
{
    public interface INIManager
    {
        event EventHandler FrameComplete;
        event EventHandler StatusChanged;
        event EventHandler StatisticsReady;

        void StartGenerating();
        void StopGenerating();
        void StartRendering();
        void StopRendering();
        void SettingsChanged();
        Bitmap GetFrame();
    }

    public class StatusEventArgs : EventArgs
    {
        public StatusEventArgs(string msg)
        {
            status = msg;
        }

        public string Status
        {
            get
            {
                return status;
            }
        }

        private string status = string.Empty;
    }

    public class StatisticsEventArgs : EventArgs
    {
        public StatisticsEventArgs(string msg)
        {
            statistics = msg;
        }

        public string Statistics
        {
            get
            {
                return statistics;
            }
        }

        private string statistics = string.Empty;
    }

    class NIManager : INIManager
    {
        #region INIManager
        
        public event EventHandler FrameComplete;
        public event EventHandler StatusChanged;
        public event EventHandler StatisticsReady;

        public void StartGenerating()
        {
            if (!isGenerating)
            {
                lock (lockGenerating)
                {
                    if (!isGenerating)
                    {
                        StartGeneratingWithNoLock();
                    }
                }
            }
        }

        public void StopGenerating()
        {
            if (isGenerating)
            {
                lock (lockGenerating)
                {
                    if (isGenerating)
                    {
                        if (isRendering)
                        {
                            lock (lockRendering)
                            {
                                if (isRendering)
                                {
                                    StopRenderingWithNoLock();
                                }
                            }
                        }
                        
                        isGenerating = false;
                        this.userGenerator.StopGenerating();
                    }
                }
            }
        }

        public void StartRendering()
        {
            if (!isGenerating)
            {
                lock (lockGenerating)
                {
                    if (!isGenerating)
                    {
                        StartGeneratingWithNoLock();
                    }

                    if (!isRendering)
                    {
                        lock (lockRendering)
                        {
                            if (!isRendering)
                            {
                                this.renderTreadTerminate = false;
                                this.renderThread.Start();

                                isRendering = true;
                            }
                        }
                    }
                }
            }
        }

        public void StopRendering()
        {
            if (isRendering)
            {
                lock (lockRendering)
                {
                    if (isRendering)
                    {
                        StopRenderingWithNoLock();
                    }
                }
            }
        }

        public void SettingsChanged()
        {
            settings = Configuration.Settings;
        }

        public Bitmap GetFrame()
        {
            return bitmap;
        }

        #endregion INIManager

        public static INIManager CreateInstance()
        {
            return new NIManager();
        }

        public NIManager()
        { 
            try
            {
                this.settings = Configuration.Settings;

                this.context = Context.CreateFromXmlFile(Configuration.OPENNI_CONFIG_FILE, out scriptNode);
                this.depth = this.context.FindExistingNode(NodeType.Depth) as DepthGenerator;
                if (this.depth == null)
                {
                    throw new Exception("Viewer must have a depth node!");
                }
                this.histogram = new int[this.depth.DeviceMaxDepth];

                this.userGenerator = new UserGenerator(this.context);
                this.userGenerator.NewUser += NewUser;
                this.userGenerator.LostUser += LostUser;

                this.skeletonCapbility = this.userGenerator.SkeletonCapability;
                if (!this.skeletonCapbility.DoesNeedPoseForCalibration)
                {
                    throw new Exception("Should not need pose for calibration!");
                }

                this.skeletonCapbility.CalibrationComplete += CalibrationComplete;
                this.skeletonCapbility.SetSkeletonProfile(settings.GeneratingSettings.SkeletonProfile);

                this.usersInScene = new Dictionary<int, NIUserInfoEx>();
                this.usersInPast = new List<NIUserInfo>();

                this.bitmap = new Bitmap((int)this.depth.MapOutputMode.XRes, (int)this.depth.MapOutputMode.YRes/*, System.Drawing.Imaging.PixelFormat.Format24bppRgb*/);
            
                this.renderThread = new Thread(RenderThread);

                this.statisticsTimer = new Timer(StatisticsTimerCallback);
                this.statisticsTimer.Change(Configuration.STATISTICS_INTERVAL_MS, Timeout.Infinite);

                Diagnostics.TraceDebug("NIManager created");
            }
            catch (Exception)
            {
                Diagnostics.TraceDebug("NIManager creation failed");
                throw;
            }
        }

        private void NewUser(object sender, NewUserEventArgs e)
        {
            string msg = string.Format("people [{0}] detected, start calibration ...", e.ID);
            StatusChanged(this, new StatusEventArgs(msg));
            Diagnostics.TraceDebug(msg);

            this.skeletonCapbility.RequestCalibration(e.ID, true);
        }

        private NIUserInfoEx CreateUserEx()
        {
            NIUserInfoEx userEx = new NIUserInfoEx();
            userEx.User = new NIUserInfo();
            userEx.User.Id = Guid.NewGuid();
            userEx.User.InTick = 0;
            userEx.User.OutTick = 0;

            userEx.Joints = new Dictionary<SkeletonJoint, SkeletonJointPosition>();

            return userEx;
        }

        private void CalibrationComplete(object sender, CalibrationProgressEventArgs e)
        {
            if (e.Status == CalibrationStatus.OK)
            {
                string msg = string.Format("people [{0}] calibrated, start tracking ...", e.ID);
                StatusChanged(this, new StatusEventArgs(msg));
                Diagnostics.TraceDebug(msg);

                this.skeletonCapbility.StartTracking(e.ID);

                NIUserInfoEx userEx = CreateUserEx();
                userEx.User.InTick = DateTime.Now.Ticks;
                this.usersInScene.Add(e.ID, userEx);
            }
            else if (e.Status != CalibrationStatus.ManualAbort)
            {
                string msg = string.Format("people [{0}] calibration failed, restart calibration ...", e.ID);
                StatusChanged(this, new StatusEventArgs(msg));
                Diagnostics.TraceDebug(msg);

                this.skeletonCapbility.RequestCalibration(e.ID, true);
            }
        }

        private void LostUser(object sender, UserLostEventArgs e)
        {
            string msg = string.Format("people [{0}] lost", e.ID);
            StatusChanged(this, new StatusEventArgs(msg));
            Diagnostics.TraceDebug(msg);

            NIUserInfo user = usersInScene[e.ID].User;
            user.OutTick = DateTime.Now.Ticks;

            usersInScene.Remove(e.ID);
            usersInPast.Add(user);
        }

        private unsafe void CalcHist(DepthMetaData depthMD)
        {
            // reset
            for (int i = 0; i < this.histogram.Length; ++i)
                this.histogram[i] = 0;

            ushort* pDepth = (ushort*)depthMD.DepthMapPtr.ToPointer();

            int points = 0;
            for (int y = 0; y < depthMD.YRes; ++y)
            {
                for (int x = 0; x < depthMD.XRes; ++x, ++pDepth)
                {
                    ushort depthVal = *pDepth;
                    if (depthVal != 0)
                    {
                        this.histogram[depthVal]++;
                        points++;
                    }
                }
            }

            for (int i = 1; i < this.histogram.Length; i++)
            {
                this.histogram[i] += this.histogram[i - 1];
            }

            if (points > 0)
            {
                for (int i = 1; i < this.histogram.Length; i++)
                {
                    this.histogram[i] = (int)(256 * (1.0f - (this.histogram[i] / (float)points)));
                }
            }
        }

        private void GetJoint(int user, SkeletonJoint joint)
        {
            SkeletonJointPosition pos = this.skeletonCapbility.GetSkeletonJointPosition(user, joint);
            if (pos.Position.Z == 0)
            {
                pos.Confidence = 0;
            }
            else
            {
                pos.Position = this.depth.ConvertRealWorldToProjective(pos.Position);
            }

            usersInScene[user].Joints[joint] = pos;
        }

        private void GetJoints(int user)
        {
            GetJoint(user, SkeletonJoint.Head);
            GetJoint(user, SkeletonJoint.Neck);

            GetJoint(user, SkeletonJoint.LeftShoulder);
            GetJoint(user, SkeletonJoint.LeftElbow);
            GetJoint(user, SkeletonJoint.LeftHand);

            GetJoint(user, SkeletonJoint.RightShoulder);
            GetJoint(user, SkeletonJoint.RightElbow);
            GetJoint(user, SkeletonJoint.RightHand);

            GetJoint(user, SkeletonJoint.Torso);

            GetJoint(user, SkeletonJoint.LeftHip);
            GetJoint(user, SkeletonJoint.LeftKnee);
            GetJoint(user, SkeletonJoint.LeftFoot);

            GetJoint(user, SkeletonJoint.RightHip);
            GetJoint(user, SkeletonJoint.RightKnee);
            GetJoint(user, SkeletonJoint.RightFoot);
        }

        private void DrawLine(Graphics g, Color color, Dictionary<SkeletonJoint, SkeletonJointPosition> dict, SkeletonJoint j1, SkeletonJoint j2)
        {
            Point3D pos1 = dict[j1].Position;
            Point3D pos2 = dict[j2].Position;

            if (dict[j1].Confidence == 0 || dict[j2].Confidence == 0)
                return;

            g.DrawLine(new Pen(color),
                        new Point((int)pos1.X, (int)pos1.Y),
                        new Point((int)pos2.X, (int)pos2.Y));

        }
        
        private void DrawSkeleton(Graphics g, Color color, int user)
        {
            GetJoints(user);
            Dictionary<SkeletonJoint, SkeletonJointPosition> dict = usersInScene[user].Joints;

            DrawLine(g, color, dict, SkeletonJoint.Head, SkeletonJoint.Neck);

            DrawLine(g, color, dict, SkeletonJoint.LeftShoulder, SkeletonJoint.Torso);
            DrawLine(g, color, dict, SkeletonJoint.RightShoulder, SkeletonJoint.Torso);

            DrawLine(g, color, dict, SkeletonJoint.Neck, SkeletonJoint.LeftShoulder);
            DrawLine(g, color, dict, SkeletonJoint.LeftShoulder, SkeletonJoint.LeftElbow);
            DrawLine(g, color, dict, SkeletonJoint.LeftElbow, SkeletonJoint.LeftHand);

            DrawLine(g, color, dict, SkeletonJoint.Neck, SkeletonJoint.RightShoulder);
            DrawLine(g, color, dict, SkeletonJoint.RightShoulder, SkeletonJoint.RightElbow);
            DrawLine(g, color, dict, SkeletonJoint.RightElbow, SkeletonJoint.RightHand);

            DrawLine(g, color, dict, SkeletonJoint.LeftHip, SkeletonJoint.Torso);
            DrawLine(g, color, dict, SkeletonJoint.RightHip, SkeletonJoint.Torso);
            DrawLine(g, color, dict, SkeletonJoint.LeftHip, SkeletonJoint.RightHip);

            DrawLine(g, color, dict, SkeletonJoint.LeftHip, SkeletonJoint.LeftKnee);
            DrawLine(g, color, dict, SkeletonJoint.LeftKnee, SkeletonJoint.LeftFoot);

            DrawLine(g, color, dict, SkeletonJoint.RightHip, SkeletonJoint.RightKnee);
            DrawLine(g, color, dict, SkeletonJoint.RightKnee, SkeletonJoint.RightFoot);
        }

        private unsafe void RenderThread()
        {
            DepthMetaData depthMD = new DepthMetaData();

            while (this.renderTreadTerminate)
            {
                try
                {
                    this.context.WaitOneUpdateAll(this.depth);
                }
                catch (Exception)
                {
                }

                this.depth.GetMetaData(depthMD);

                CalcHist(depthMD);

                lock (this)
                {
                    Rectangle rect = new Rectangle(0, 0, this.bitmap.Width, this.bitmap.Height);
                    BitmapData data = this.bitmap.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);


                    if (settings.RenderingSettings.DrawPixels)
                    {
                        ushort* pDepth = (ushort*)this.depth.DepthMapPtr.ToPointer();
                        ushort* pLabels = (ushort*)this.userGenerator.GetUserPixels(0).LabelMapPtr.ToPointer();

                        // set pixels
                        for (int y = 0; y < depthMD.YRes; ++y)
                        {
                            byte* pDest = (byte*)data.Scan0.ToPointer() + y * data.Stride;
                            for (int x = 0; x < depthMD.XRes; ++x, ++pDepth, ++pLabels, pDest += 3)
                            {
                                pDest[0] = pDest[1] = pDest[2] = 0;

                                ushort label = *pLabels;
                                if (this.settings.RenderingSettings.DrawBackground || *pLabels != 0)
                                {
                                    Color labelColor = Color.White;
                                    if (label != 0)
                                    {
                                        labelColor = colors[label % ncolors];
                                    }

                                    byte pixel = (byte)this.histogram[*pDepth];
                                    pDest[0] = (byte)(pixel * (labelColor.B / 256.0));
                                    pDest[1] = (byte)(pixel * (labelColor.G / 256.0));
                                    pDest[2] = (byte)(pixel * (labelColor.R / 256.0));
                                }
                            }
                        }
                    }
                    this.bitmap.UnlockBits(data);

                    Graphics g = Graphics.FromImage(this.bitmap);
                    int[] users = this.userGenerator.GetUsers();
                    foreach (int user in users)
                    {
                        if (settings.RenderingSettings.PrintID)
                        {
                            Point3D com = this.userGenerator.GetCoM(user);
                            com = this.depth.ConvertRealWorldToProjective(com);

                            string label = "";
                            if (!settings.RenderingSettings.PrintState)
                                label += user;
                            else if (this.skeletonCapbility.IsTracking(user))
                                label += user + " - Tracking";
                            else if (this.skeletonCapbility.IsCalibrating(user))
                                label += user + " - Calibrating...";
                            else
                                label += user + " - Looking for pose";

                            g.DrawString(label, new Font("Arial", 6), new SolidBrush(anticolors[user % ncolors]), com.X, com.Y);

                        }

                        if (settings.RenderingSettings.DrawSkeleton && this.skeletonCapbility.IsTracking(user))
                            //                        if (this.skeletonCapbility.IsTracking(user))
                            DrawSkeleton(g, anticolors[user % ncolors], user);

                    }
                    g.Dispose();
                }

                FrameComplete(this, null);
            }
        }

        private void StopRenderingWithNoLock()
        {
            this.isRendering = false;

            this.renderTreadTerminate = true;
            this.renderThread.Join();
        }

        private void StartGeneratingWithNoLock()
        {
            this.userGenerator.StartGenerating();
            isGenerating = true;
        }

        private void StatisticsTimerCallback(object state)
        {
            this.statisticsTimer.Change(Timeout.Infinite, Timeout.Infinite);

            StatisticsReady(this, new StatisticsEventArgs(string.Empty));

            this.statisticsTimer.Change(Configuration.STATISTICS_INTERVAL_MS, Timeout.Infinite);
        }

        private bool isGenerating = false;
        private bool isRendering = false;
        private object lockGenerating = new object();
        private object lockRendering = new object();

        private Context context = null;
        private DepthGenerator depth = null;
        private UserGenerator userGenerator = null;
        private SkeletonCapability skeletonCapbility = null;

        private Dictionary<int, NIUserInfoEx> usersInScene = null;
        private List<NIUserInfo> usersInPast = null;
        private UtilitySettings settings = null;
        private Timer statisticsTimer = null;
        
        private Thread renderThread;
        private bool renderTreadTerminate = true;
        private Bitmap bitmap;
        private int[] histogram;

        private ScriptNode scriptNode;

        private Color[] colors = { Color.Red, Color.Blue, Color.ForestGreen, Color.Yellow, Color.Orange, Color.Purple, Color.White };
        private Color[] anticolors = { Color.Green, Color.Orange, Color.Red, Color.Purple, Color.Blue, Color.Yellow, Color.Black };
        private int ncolors = 6;
    }

    class NIUserInfo
    {
        public Guid Id { get; set; }
        public long InTick { get; set; }
        public long OutTick { get; set; }
    }

    class NIUserInfoEx
    {
        public NIUserInfo User { get; set; }
        public Dictionary<SkeletonJoint, SkeletonJointPosition> Joints { get; set; }
    }


}
