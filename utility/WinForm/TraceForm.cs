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
    public partial class TraceForm : Form
    {
        
        public TraceForm(INIManager manager)
        {
            InitializeComponent();

            niManager = manager;
            niManager.StatusChanged += new EventHandler(StatusChanged);
            niManager.StatisticsReady += new EventHandler(StatisticsReady);
        }

        private void StatusChanged(object sender, EventArgs e)
        {
            this.tbTrace.AppendText((e as StatusEventArgs).StatusMessage + "\n");
        }

        private void StatisticsReady(object sender, EventArgs e)
        {
            this.llStatistics.Text = (e as StatisticsEventArgs).StatisticsMessage;
        }

        private INIManager niManager = null;
    }
}
