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
        private INIManager niManager = null;

        public MainForm(INIManager manager)
        {
            InitializeComponent();
            
            
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            lock (this)
            {
                e.Graphics.DrawImage(this.bitmap,
                    this.CanvasPanel.Location.X,
                    this.CanvasPanel.Location.Y,
                    this.CanvasPanel.Size.Width,
                    this.CanvasPanel.Size.Height);
            }
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            this.shouldRun = false;
            this.readerThread.Join();
            base.OnClosing(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
            {
                Close();
            }
            switch (e.KeyChar)
            {
                case (char)27:
                    break;
                case 'b':
                    this.shouldDrawBackground = !this.shouldDrawBackground;
                    break;
                case 'x':
                    this.shouldDrawPixels = !this.shouldDrawPixels;
                    break;
                case 's':
                    this.shouldDrawSkeleton = !this.shouldDrawSkeleton;
                    break;
                case 'i':
                    this.shouldPrintID = !this.shouldPrintID;
                    break;
                case 'l':
                    this.shouldPrintState = !this.shouldPrintState;
                    break;

            }
            base.OnKeyPress(e);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //Don't allow the background to paint
        }
    }
}
