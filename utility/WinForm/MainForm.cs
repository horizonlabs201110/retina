﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace Com.Imola.Retina.Utility.WinForm
{
    public partial class MainForm : Form
    {
        public MainForm(INIManager manager)
        {
            InitializeComponent();
            
            niManager = manager;

            traceForm = new TraceForm(niManager);
            traceForm.Hide();
            isGenerating = false;
            canvasForm = new CanvasForm(niManager);
            canvasForm.Hide();
            isRendering = false;
        }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ApplyConfiguration();
            btGenerating.Text = BUTTON_TEXT_GENERATING_START;
            btRendering.Text = BUTTON_TEXT_RENDERING_START;
        }

        private void btGenerating_Click(object sender, EventArgs e)
        {
            if (isGenerating)
            {
                niManager.StopGenerating();
                btGenerating.Text = BUTTON_TEXT_GENERATING_START;
                isGenerating = false;
            }
            else
            {
                niManager.StartGenerating();
                btGenerating.Text = BUTTON_TEXT_GENERATING_STOP;
                isGenerating = true;
            }
        }

        private void btTrace_Click(object sender, EventArgs e)
        {
            if (traceForm.Visible)
            {
                traceForm.Hide();
            }
            else
            {
                traceForm.Show();
            }
        }

        private void btRendering_Click(object sender, EventArgs e)
        {
            if (isRendering)
            {
                niManager.StopRendering();
                canvasForm.Hide();
                btRendering.Text = BUTTON_TEXT_RENDERING_START;
                isRendering = false;
            }
            else
            {
                niManager.StartRendering();
                canvasForm.Show();
                btRendering.Text = BUTTON_TEXT_RENDERING_STOP;
                isRendering = true;
            }
        }

        private void ApplyConfiguration()
        {
            cbSkeletonProfile.SelectedText = Configuration.Settings.GeneratingSettings.SkeletonProfile.ToString();
            tbPeopleInKey.Text = Configuration.Settings.GeneratingSettings.KeyboardMapping[KeyboardTrigger.PeopleIn].ToString();
            tbPeopleOutKey.Text = Configuration.Settings.GeneratingSettings.KeyboardMapping[KeyboardTrigger.PeopleOut].ToString();

            cbDrawBackground.Checked = Configuration.Settings.RenderingSettings.DrawBackground;
            cbDrawPixels.Checked = Configuration.Settings.RenderingSettings.DrawPixels;
            cbDrawSkeleton.Checked = Configuration.Settings.RenderingSettings.DrawSkeleton;
            cbPrintID.Checked = Configuration.Settings.RenderingSettings.PrintID;
            cbPrintState.Checked = Configuration.Settings.RenderingSettings.PrintState;
        }

        private void cbSkeletonProfile_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                OpenNI.SkeletonProfile profile = (OpenNI.SkeletonProfile)Enum.Parse(
                    typeof(OpenNI.SkeletonProfile),
                    cbSkeletonProfile.SelectedItem.ToString());

                UtilitySettings newSettings = (UtilitySettings)Configuration.Settings.Clone();
                newSettings.GeneratingSettings.SkeletonProfile = profile;
                Configuration.Settings = newSettings;
                niManager.SettingsChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Skeleton Profile Wrong, {0}", ex.Message));
                cbSkeletonProfile.SelectedText = Configuration.Settings.GeneratingSettings.SkeletonProfile.ToString();
            }
        }

        private void tbPeopleInKey_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ushort peopleInKey = (ushort)int.Parse(tbPeopleInKey.Text);

                UtilitySettings newSettings = (UtilitySettings)Configuration.Settings.Clone();
                newSettings.GeneratingSettings.KeyboardMapping[KeyboardTrigger.PeopleIn] = peopleInKey;
                Configuration.Settings = newSettings;
                niManager.SettingsChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("PeopleIn Key Wrong, {0}", ex.Message));
                tbPeopleInKey.Text = Configuration.Settings.GeneratingSettings.KeyboardMapping[KeyboardTrigger.PeopleIn].ToString();
            }
        }

        private void tbPeopleOutKey_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ushort peopleOutKey = (ushort)int.Parse(tbPeopleOutKey.Text);

                UtilitySettings newSettings = (UtilitySettings)Configuration.Settings.Clone();
                newSettings.GeneratingSettings.KeyboardMapping[KeyboardTrigger.PeopleOut] = peopleOutKey;
                Configuration.Settings = newSettings;
                niManager.SettingsChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("PeopleOut Key Wrong, {0}", ex.Message));
                tbPeopleOutKey.Text = Configuration.Settings.GeneratingSettings.KeyboardMapping[KeyboardTrigger.PeopleOut].ToString();
            }
        }

        private void cbDrawBackground_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UtilitySettings newSettings = (UtilitySettings)Configuration.Settings.Clone();
                newSettings.RenderingSettings.DrawBackground = cbDrawBackground.Checked;
                Configuration.Settings = newSettings;
                niManager.SettingsChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("DrawBackground Wrong, {0}", ex.Message));
                cbDrawBackground.Checked = Configuration.Settings.RenderingSettings.DrawBackground;
            }
        }

        private void cbDrawPixels_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UtilitySettings newSettings = (UtilitySettings)Configuration.Settings.Clone();
                newSettings.RenderingSettings.DrawPixels = cbDrawPixels.Checked;
                Configuration.Settings = newSettings;
                niManager.SettingsChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("DrawPixels Wrong, {0}", ex.Message));
                cbDrawPixels.Checked = Configuration.Settings.RenderingSettings.DrawPixels;
            }
        }

        private void cbDrawSkeleton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UtilitySettings newSettings = (UtilitySettings)Configuration.Settings.Clone();
                newSettings.RenderingSettings.DrawSkeleton = cbDrawSkeleton.Checked;
                Configuration.Settings = newSettings;
                niManager.SettingsChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("DrawSkeleton Wrong, {0}", ex.Message));
                cbDrawSkeleton.Checked = Configuration.Settings.RenderingSettings.DrawSkeleton;
            }
        }

        private void cbPrintID_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UtilitySettings newSettings = (UtilitySettings)Configuration.Settings.Clone();
                newSettings.RenderingSettings.PrintID = cbPrintID.Checked;
                Configuration.Settings = newSettings;
                niManager.SettingsChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("PrintID Wrong, {0}", ex.Message));
                cbPrintID.Checked = Configuration.Settings.RenderingSettings.PrintID;
            }
        }

        private void cbPrintState_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UtilitySettings newSettings = (UtilitySettings)Configuration.Settings.Clone();
                newSettings.RenderingSettings.PrintState = cbPrintState.Checked;
                Configuration.Settings = newSettings;
                niManager.SettingsChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("PrintState Wrong, {0}", ex.Message));
                cbPrintState.Checked = Configuration.Settings.RenderingSettings.PrintState;
            }
        }

        private INIManager niManager = null;

        private bool isGenerating = false;
        private bool isRendering = false;
        
        private Form traceForm = null;
        private Form canvasForm = null;

        private const string BUTTON_TEXT_GENERATING_START = "start generating";
        private const string BUTTON_TEXT_GENERATING_STOP = "stop generating";
        private const string BUTTON_TEXT_RENDERING_START = "start rendering";
        private const string BUTTON_TEXT_RENDERING_STOP = "stop rendering";
    }
}
