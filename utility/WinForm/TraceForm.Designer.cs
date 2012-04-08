namespace Com.Imola.Retina.Utility.WinForm
{
    partial class TraceForm
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
            this.tlpControl = new System.Windows.Forms.TableLayoutPanel();
            this.tbTrace = new System.Windows.Forms.TextBox();
            this.llStatistics = new System.Windows.Forms.Label();
            this.tlpControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpControl
            // 
            this.tlpControl.ColumnCount = 1;
            this.tlpControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpControl.Controls.Add(this.tbTrace, 0, 1);
            this.tlpControl.Controls.Add(this.llStatistics, 0, 0);
            this.tlpControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpControl.Location = new System.Drawing.Point(0, 0);
            this.tlpControl.Name = "tlpControl";
            this.tlpControl.RowCount = 2;
            this.tlpControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.46721F));
            this.tlpControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80.53278F));
            this.tlpControl.Size = new System.Drawing.Size(528, 488);
            this.tlpControl.TabIndex = 0;
            // 
            // tbTrace
            // 
            this.tbTrace.AcceptsReturn = true;
            this.tbTrace.Location = new System.Drawing.Point(3, 97);
            this.tbTrace.Multiline = true;
            this.tbTrace.Name = "tbTrace";
            this.tbTrace.ReadOnly = true;
            this.tbTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbTrace.Size = new System.Drawing.Size(522, 388);
            this.tbTrace.TabIndex = 0;
            this.tbTrace.WordWrap = false;
            // 
            // llStatistics
            // 
            this.llStatistics.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.llStatistics.AutoSize = true;
            this.llStatistics.Location = new System.Drawing.Point(264, 40);
            this.llStatistics.Name = "llStatistics";
            this.llStatistics.Size = new System.Drawing.Size(0, 13);
            this.llStatistics.TabIndex = 1;
            this.llStatistics.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TraceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(528, 488);
            this.Controls.Add(this.tlpControl);
            this.Name = "TraceForm";
            this.Text = "Utility Trace";
            this.tlpControl.ResumeLayout(false);
            this.tlpControl.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpControl;
        private System.Windows.Forms.TextBox tbTrace;
        private System.Windows.Forms.Label llStatistics;
    }
}