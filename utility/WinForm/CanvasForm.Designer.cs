namespace Com.Imola.Retina.Utility.WinForm
{
    partial class CanvasForm
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
            this.plCanvas = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // plCanvas
            // 
            this.plCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plCanvas.Location = new System.Drawing.Point(0, 0);
            this.plCanvas.Name = "plCanvas";
            this.plCanvas.Size = new System.Drawing.Size(602, 439);
            this.plCanvas.TabIndex = 0;
            this.plCanvas.Visible = false;
            // 
            // CanvasForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 439);
            this.Controls.Add(this.plCanvas);
            this.Name = "CanvasForm";
            this.Text = "Utility Canvas";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel plCanvas;
    }
}