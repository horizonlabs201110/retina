using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Com.Imola.Retina.Utility
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                Application.Run(new MainForm());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "UserTracker", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Application.
        }
    }
}
