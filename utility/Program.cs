using System;
using System.Windows.Forms;
using System.Threading;
using Com.Imola.Retina.Utility.WinForm;

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
            Diagnostics.Trace(TraceLevel.Information, "Utility started");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
            
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            niManager = NIManager.CreateInstance();
            Application.Run(new MainForm(niManager));
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (niManager != null)
            {
                //niManager
                niManager = null;
            }
            Diagnostics.Trace(TraceLevel.Information, "Utility exit");
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Diagnostics.Trace(TraceLevel.Error, "Unhandled exception encountered, {0}", e.Exception.ToString());
            MessageBox.Show("Exit with unhandled error: " + e.Exception.Message, "Utility", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        private static INIManager niManager = null;
    }
}
