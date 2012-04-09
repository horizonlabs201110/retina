using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Com.Imola.Retina.Utility.WinForm
{
    public partial class CanvasForm : Form
    {
        public CanvasForm(INIManager manager)
        {
            
            InitializeComponent();

            niManager = manager;
            niManager.FrameComplete += new EventHandler(FrameComplete);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Bitmap bitmap = niManager.GetFrame();
            if (bitmap != null)
            {
                lock (bitmap)
                {
                    e.Graphics.DrawImage(bitmap,
                        this.plCanvas.Location.X,
                        this.plCanvas.Location.Y,
                        this.plCanvas.Size.Width,
                        this.plCanvas.Size.Height);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //Don't allow the background to paint
        }

        private void FrameComplete(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private INIManager niManager = null;
    }
}
