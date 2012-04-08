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
        public CanvasForm()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            /*
            lock (this)
            {
                e.Graphics.DrawImage(this.bitmap,
                    this.CanvasPanel.Location.X,
                    this.CanvasPanel.Location.Y,
                    this.CanvasPanel.Size.Width,
                    this.CanvasPanel.Size.Height);
            }*/
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //Don't allow the background to paint
        }
    }
}
