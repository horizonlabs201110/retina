namespace Com.Imola.Retina.Utility
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
            this.CanvasPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // CanvasPanel
            // 
            this.CanvasPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CanvasPanel.Location = new System.Drawing.Point(0, 0);
            this.CanvasPanel.Name = "CanvasPanel";
            this.CanvasPanel.Size = new System.Drawing.Size(602, 439);
            this.CanvasPanel.TabIndex = 0;
            this.CanvasPanel.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 439);
            this.Controls.Add(this.CanvasPanel);
            this.Name = "MainForm";
            this.Text = "Utility";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel CanvasPanel;
    }
}

