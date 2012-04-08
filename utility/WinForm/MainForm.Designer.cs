namespace Com.Imola.Retina.Utility.WinForm
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tpGeneral = new System.Windows.Forms.TabPage();
            this.tlpControl = new System.Windows.Forms.TableLayoutPanel();
            this.gbGenerating = new System.Windows.Forms.GroupBox();
            this.gbRendering = new System.Windows.Forms.GroupBox();
            this.cbSkeletonProfile = new System.Windows.Forms.ComboBox();
            this.btGenerating = new System.Windows.Forms.Button();
            this.btTrace = new System.Windows.Forms.Button();
            this.btRendering = new System.Windows.Forms.Button();
            this.cbDrawBackground = new System.Windows.Forms.CheckBox();
            this.cbDrawPixels = new System.Windows.Forms.CheckBox();
            this.cbDrawSkeleton = new System.Windows.Forms.CheckBox();
            this.cbPrintID = new System.Windows.Forms.CheckBox();
            this.cbPrintState = new System.Windows.Forms.CheckBox();
            this.tabControl.SuspendLayout();
            this.tpGeneral.SuspendLayout();
            this.tlpControl.SuspendLayout();
            this.gbGenerating.SuspendLayout();
            this.gbRendering.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tpGeneral);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(475, 471);
            this.tabControl.TabIndex = 0;
            // 
            // tpGeneral
            // 
            this.tpGeneral.Controls.Add(this.tlpControl);
            this.tpGeneral.Location = new System.Drawing.Point(4, 22);
            this.tpGeneral.Name = "tpGeneral";
            this.tpGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tpGeneral.Size = new System.Drawing.Size(467, 445);
            this.tpGeneral.TabIndex = 0;
            this.tpGeneral.Text = "General";
            this.tpGeneral.UseVisualStyleBackColor = true;
            // 
            // tlpControl
            // 
            this.tlpControl.ColumnCount = 1;
            this.tlpControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpControl.Controls.Add(this.gbGenerating, 0, 0);
            this.tlpControl.Controls.Add(this.gbRendering, 0, 1);
            this.tlpControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpControl.Location = new System.Drawing.Point(3, 3);
            this.tlpControl.Name = "tlpControl";
            this.tlpControl.RowCount = 2;
            this.tlpControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpControl.Size = new System.Drawing.Size(461, 439);
            this.tlpControl.TabIndex = 0;
            // 
            // gbGenerating
            // 
            this.gbGenerating.Controls.Add(this.btTrace);
            this.gbGenerating.Controls.Add(this.btGenerating);
            this.gbGenerating.Controls.Add(this.cbSkeletonProfile);
            this.gbGenerating.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbGenerating.Location = new System.Drawing.Point(3, 3);
            this.gbGenerating.Name = "gbGenerating";
            this.gbGenerating.Size = new System.Drawing.Size(455, 213);
            this.gbGenerating.TabIndex = 0;
            this.gbGenerating.TabStop = false;
            this.gbGenerating.Text = "Generating";
            // 
            // gbRendering
            // 
            this.gbRendering.Controls.Add(this.cbPrintState);
            this.gbRendering.Controls.Add(this.cbPrintID);
            this.gbRendering.Controls.Add(this.cbDrawSkeleton);
            this.gbRendering.Controls.Add(this.cbDrawPixels);
            this.gbRendering.Controls.Add(this.cbDrawBackground);
            this.gbRendering.Controls.Add(this.btRendering);
            this.gbRendering.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbRendering.Location = new System.Drawing.Point(3, 222);
            this.gbRendering.Name = "gbRendering";
            this.gbRendering.Size = new System.Drawing.Size(455, 214);
            this.gbRendering.TabIndex = 1;
            this.gbRendering.TabStop = false;
            this.gbRendering.Text = "Rendering";
            // 
            // cbSkeletonProfile
            // 
            this.cbSkeletonProfile.FormattingEnabled = true;
            this.cbSkeletonProfile.Items.AddRange(new object[] {
            "None",
            "All",
            "Upper",
            "Lower",
            "HeadAndHands"});
            this.cbSkeletonProfile.Location = new System.Drawing.Point(31, 37);
            this.cbSkeletonProfile.Name = "cbSkeletonProfile";
            this.cbSkeletonProfile.Size = new System.Drawing.Size(121, 21);
            this.cbSkeletonProfile.TabIndex = 0;
            // 
            // btGenerating
            // 
            this.btGenerating.Location = new System.Drawing.Point(31, 175);
            this.btGenerating.Name = "btGenerating";
            this.btGenerating.Size = new System.Drawing.Size(102, 23);
            this.btGenerating.TabIndex = 1;
            this.btGenerating.Text = "Start Generating";
            this.btGenerating.UseVisualStyleBackColor = true;
            this.btGenerating.Click += new System.EventHandler(this.btGenerating_Click);
            // 
            // btTrace
            // 
            this.btTrace.Location = new System.Drawing.Point(139, 175);
            this.btTrace.Name = "btTrace";
            this.btTrace.Size = new System.Drawing.Size(102, 23);
            this.btTrace.TabIndex = 3;
            this.btTrace.Text = "Trace";
            this.btTrace.UseVisualStyleBackColor = true;
            this.btTrace.Click += new System.EventHandler(this.btTrace_Click);
            // 
            // btRendering
            // 
            this.btRendering.Location = new System.Drawing.Point(31, 174);
            this.btRendering.Name = "btRendering";
            this.btRendering.Size = new System.Drawing.Size(102, 23);
            this.btRendering.TabIndex = 0;
            this.btRendering.Text = "Start Rendering";
            this.btRendering.UseVisualStyleBackColor = true;
            this.btRendering.Click += new System.EventHandler(this.btRendering_Click);
            // 
            // cbDrawBackground
            // 
            this.cbDrawBackground.AutoSize = true;
            this.cbDrawBackground.Location = new System.Drawing.Point(31, 30);
            this.cbDrawBackground.Name = "cbDrawBackground";
            this.cbDrawBackground.Size = new System.Drawing.Size(112, 17);
            this.cbDrawBackground.TabIndex = 1;
            this.cbDrawBackground.Text = "Draw Background";
            this.cbDrawBackground.UseVisualStyleBackColor = true;
            // 
            // cbDrawPixels
            // 
            this.cbDrawPixels.AutoSize = true;
            this.cbDrawPixels.Location = new System.Drawing.Point(31, 54);
            this.cbDrawPixels.Name = "cbDrawPixels";
            this.cbDrawPixels.Size = new System.Drawing.Size(81, 17);
            this.cbDrawPixels.TabIndex = 2;
            this.cbDrawPixels.Text = "Draw Pixels";
            this.cbDrawPixels.UseVisualStyleBackColor = true;
            // 
            // cbDrawSkeleton
            // 
            this.cbDrawSkeleton.AutoSize = true;
            this.cbDrawSkeleton.Location = new System.Drawing.Point(31, 78);
            this.cbDrawSkeleton.Name = "cbDrawSkeleton";
            this.cbDrawSkeleton.Size = new System.Drawing.Size(96, 17);
            this.cbDrawSkeleton.TabIndex = 3;
            this.cbDrawSkeleton.Text = "Draw Skeleton";
            this.cbDrawSkeleton.UseVisualStyleBackColor = true;
            // 
            // cbPrintID
            // 
            this.cbPrintID.AutoSize = true;
            this.cbPrintID.Location = new System.Drawing.Point(31, 102);
            this.cbPrintID.Name = "cbPrintID";
            this.cbPrintID.Size = new System.Drawing.Size(61, 17);
            this.cbPrintID.TabIndex = 4;
            this.cbPrintID.Text = "Print ID";
            this.cbPrintID.UseVisualStyleBackColor = true;
            // 
            // cbPrintState
            // 
            this.cbPrintState.AutoSize = true;
            this.cbPrintState.Location = new System.Drawing.Point(31, 126);
            this.cbPrintState.Name = "cbPrintState";
            this.cbPrintState.Size = new System.Drawing.Size(75, 17);
            this.cbPrintState.TabIndex = 5;
            this.cbPrintState.Text = "Print State";
            this.cbPrintState.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(475, 471);
            this.Controls.Add(this.tabControl);
            this.Name = "MainForm";
            this.Text = "Utility Main";
            this.tabControl.ResumeLayout(false);
            this.tpGeneral.ResumeLayout(false);
            this.tlpControl.ResumeLayout(false);
            this.gbGenerating.ResumeLayout(false);
            this.gbRendering.ResumeLayout(false);
            this.gbRendering.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tpGeneral;
        private System.Windows.Forms.TableLayoutPanel tlpControl;
        private System.Windows.Forms.GroupBox gbGenerating;
        private System.Windows.Forms.GroupBox gbRendering;
        private System.Windows.Forms.Button btTrace;
        private System.Windows.Forms.Button btGenerating;
        private System.Windows.Forms.ComboBox cbSkeletonProfile;
        private System.Windows.Forms.Button btRendering;
        private System.Windows.Forms.CheckBox cbPrintState;
        private System.Windows.Forms.CheckBox cbPrintID;
        private System.Windows.Forms.CheckBox cbDrawSkeleton;
        private System.Windows.Forms.CheckBox cbDrawPixels;
        private System.Windows.Forms.CheckBox cbDrawBackground;
    }
}

