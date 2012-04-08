using System;

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
        { }

        #region IDiagnostics

        public void Trace(TraceLevel level, string msg, params object[] args)
        {
            return;
        }

        public void TraceDebug(string msg, params object[] args)
        {
            Trace(TraceLevel.Debug, msg, args);
        }

        #endregion IDiagnostics
    }
}
