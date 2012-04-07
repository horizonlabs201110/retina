using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Imola.Retina.Utility
{
    interface IConfiguration
    {

    }

    class RenderConfig
    {
    }

    class NIConfig
    {
    }

    class KeyboardConfig
    {
    }

    class Configuration
    {
        public const string OPENNI_CONFIG_FILE = @"OpenNI.xml";


        private static IConfiguration GetProvider()
        {
            if (provider == null)
            {
                lock (providerLock)
                {
                    if (provider == null)
                    {
                        provider = new ConfigurationProvider();
                    }
                }
            }
            return provider;
        }

        private static IConfiguration provider = null;
        private static object providerLock = new object();


    }

    class ConfigurationProvider : IConfiguration
    {
        public ConfigurationProvider()
        { }

        #region IDiagnostics


        #endregion IDiagnostics
    }
}
