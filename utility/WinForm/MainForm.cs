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
            niManager = manager;
        }

        private void btGenerating_Click(object sender, EventArgs e)
        {

        }

        private void btTrace_Click(object sender, EventArgs e)
        {

        }

        private void btRendering_Click(object sender, EventArgs e)
        {

        }
    }
}
