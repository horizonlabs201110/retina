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
    interface INIManager
    {
        EventHandler RenderComplete;

        void Start();
        void Stop();
        void StartRendering();
        void StopRendering();
        void GetBitmap();
    }

    class NIManager : INIManager
    {
    
        public static INIManager CreateInstance()
        {
            return new NIManager();
        }

        public NIManager()
        {
            try
            {
                this.context = Context.CreateFromXmlFile(Utilities.OPENNI_CONFIG_FILE, out scriptNode);
                this.depth = this.context.FindExistingNode(NodeType.Depth) as DepthGenerator;
                if (this.depth == null)
                {
                    throw new Exception("Viewer must have a depth node!");
                }
                this.histogram = new int[this.depth.DeviceMaxDepth];

                this.userGenerator = new UserGenerator(this.context);
                this.userGenerator.NewUser += NewUser;
                this.userGenerator.LostUser += LostUser;

                this.poseDetectionCapability = this.userGenerator.PoseDetectionCapability;
                this.poseDetectionCapability.PoseDetected += PoseDetected;
                
                this.skeletonCapbility = this.userGenerator.SkeletonCapability;
                this.skeletonCapbility.CalibrationComplete += CalibrationComplete;
                this.skeletonCapbility.SetSkeletonProfile(SkeletonProfile.All);

                this.usersInScene = new Dictionary<int, NIUserInfoEx>();
                this.usersInPast = new List<NIUserInfo>();

                this.bitmap = new Bitmap((int)this.depth.MapOutputMode.XRes, (int)this.depth.MapOutputMode.YRes/*, System.Drawing.Imaging.PixelFormat.Format24bppRgb*/);

                this.renderThread = new Thread(RenderThread);

            }
            catch (Exception)
            {
                throw;
            }
            Diagnostics.TraceDebug("NIManager created");
            
            
            

            
            
        }

        #region INIManager

        public EventHandler RenderComplete;

        public void Start()
        {
            this.userGenerator.StartGenerating();
        }

        public void Stop()
        {
            this.userGenerator.StopGenerating();
        }

        public void StartRendering()
        {
            this.shouldRun = true;
            this.renderThread.Start();
        }

        public void StopRendering()
        {
            this.shouldRun = false;
            this.renderThread.Join();
        }

        public void GetBitmap()
        {
        }

        #endregion INIManager

        private void CalibrationComplete(object sender, CalibrationProgressEventArgs e)
        {
            if (e.Status == CalibrationStatus.OK)
            {
                this.skeletonCapbility.StartTracking(e.ID);
                this.joints.Add(e.ID, new Dictionary<SkeletonJoint, SkeletonJointPosition>());
            }
            else if (e.Status != CalibrationStatus.ManualAbort)
            {
                if (this.skeletonCapbility.DoesNeedPoseForCalibration)
                {
                    this.poseDetectionCapability.StartPoseDetection(this.skeletonCapbility.CalibrationPose, e.ID);
                }
                else
                {
                    this.skeletonCapbility.RequestCalibration(e.ID, true);
                }
            }
        }

        private void PoseDetected(object sender, PoseDetectedEventArgs e)
        {
            this.poseDetectionCapability.StopPoseDetection(e.ID);
            this.skeletonCapbility.RequestCalibration(e.ID, true);
        }

        private void NewUser(object sender, NewUserEventArgs e)
        {
            if (this.skeletonCapbility.DoesNeedPoseForCalibration)
            {
                this.poseDetectionCapability.StartPoseDetection(this.skeletonCapbility.CalibrationPose, e.ID);
            }
            else
            {
                this.skeletonCapbility.RequestCalibration(e.ID, true);
            }
        }

        private void LostUser(object sender, UserLostEventArgs e)
        {
            this.joints.Remove(e.ID);
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
            this.joints[user][joint] = pos;
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
            Dictionary<SkeletonJoint, SkeletonJointPosition> dict = this.joints[user];

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

            while (this.shouldRun)
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


                    if (this.shouldDrawPixels)
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
                                if (this.shouldDrawBackground || *pLabels != 0)
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
                        if (this.shouldPrintID)
                        {
                            Point3D com = this.userGenerator.GetCoM(user);
                            com = this.depth.ConvertRealWorldToProjective(com);

                            string label = "";
                            if (!this.shouldPrintState)
                                label += user;
                            else if (this.skeletonCapbility.IsTracking(user))
                                label += user + " - Tracking";
                            else if (this.skeletonCapbility.IsCalibrating(user))
                                label += user + " - Calibrating...";
                            else
                                label += user + " - Looking for pose";

                            g.DrawString(label, new Font("Arial", 6), new SolidBrush(anticolors[user % ncolors]), com.X, com.Y);

                        }

                        if (this.shouldDrawSkeleton && this.skeletonCapbility.IsTracking(user))
                            //                        if (this.skeletonCapbility.IsTracking(user))
                            DrawSkeleton(g, anticolors[user % ncolors], user);

                    }
                    g.Dispose();
                }
                this.RenderComplete.Invoke(this, null);
                this.Invalidate();
            }
        }


        private Context context = null;
        private DepthGenerator depth = null;
        private UserGenerator userGenerator = null;
        private SkeletonCapability skeletonCapbility = null;
        private PoseDetectionCapability poseDetectionCapability = null;

        private Dictionary<int, NIUserInfoEx> usersInScene = null;
        private List<NIUserInfo> usersInPast = null;

        private Thread renderThread;
        private bool shouldRun;
        private Bitmap bitmap;
        private int[] histogram;

        private ScriptNode scriptNode;

        private Dictionary<int, Dictionary<SkeletonJoint, SkeletonJointPosition>> userJoints;


        private bool shouldDrawPixels = true;
        private bool shouldDrawBackground = true;
        private bool shouldPrintID = true;
        private bool shouldPrintState = true;
        private bool shouldDrawSkeleton = true;

        private Color[] colors = { Color.Red, Color.Blue, Color.ForestGreen, Color.Yellow, Color.Orange, Color.Purple, Color.White };
        private Color[] anticolors = { Color.Green, Color.Orange, Color.Red, Color.Purple, Color.Blue, Color.Yellow, Color.Black };
        private int ncolors = 6;
    }

    class NIUserInfo
    {
        public Guid Id { get; set; }
        public Int32 InTick { get; set; }
        public Int32 OutTick { get; set; }
    }

    class NIUserInfoEx
    {
        public NIUserInfo User { get; set; }
        public Dictionary<SkeletonJoint, SkeletonJointPosition> Joints { get; set; }
    }


}
