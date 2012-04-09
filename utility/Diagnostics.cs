using System;
using System.IO;

namespace Com.Imola.Retina.Utility
{
    enum TraceLevel
    {
        Debug = 0,
        Information = 1,
        Warning = 2,
        Error = 3
    }

    interface IDiagnostics
    {
        void Trace(TraceLevel level, string msg, params object[] args);
        void TraceDebug(string msg, params object[] args);
    }

    class Diagnostics
    {
        public static void Trace(TraceLevel level, string msg, params object[] args)
        {
            GetProvider().Trace(level, msg, args);
        }

        public static void TraceDebug(string msg, params object[] args)
        {
            GetProvider().TraceDebug(msg, args);
        }

        private static IDiagnostics GetProvider()
        {
            if (provider == null)
            {
                lock (providerLock)
                {
                    if (provider == null)
                    {
                        provider = new DiagnosticsProvider();
                    }
                }
            }
            return provider;
        }

        private static IDiagnostics provider = null;
        private static object providerLock = new object();
    }

    class DiagnosticsProvider : IDiagnostics
    {
        public DiagnosticsProvider()
        {
            swLogPath = Path.Combine(Environment.CurrentDirectory, "utility.log");
            swLog = new StreamWriter(swLogPath, true, System.Text.Encoding.UTF8);
        }

        #region IDiagnostics

        public void Trace(TraceLevel level, string msg, params object[] args)
        {
            if (level >= Configuration.Settings.DiagnosticsSettings.TraceLevel)
            {
                string log =  string.Format("[{0}] [{1}], {2}",
                    DateTime.Now.ToString(),
                    level.ToString(),
                    string.Format(msg, args));

                swLog.WriteLine(log);
                swLog.Flush();
            }
            return;
        }

        public void TraceDebug(string msg, params object[] args)
        {
            Trace(TraceLevel.Debug, msg, args);
        }

        #endregion IDiagnostics

        private StreamWriter swLog = null;
        private string swLogPath = string.Empty;
    }
}