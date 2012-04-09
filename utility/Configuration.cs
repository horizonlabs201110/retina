using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNI;

namespace Com.Imola.Retina.Utility
{
    enum SettingKeys
    {
        drawBackground = 0,
        drawPixels = 1,
        drawSkeleton = 2,
        printID = 3,
        printState = 4,
        skeletonProfile = 5,
        peopleInKey = 6,
        peopleOutKey = 7,
        traceLevel = 8
    }

    enum KeyboardTrigger
    {
        PeopleIn = 0,
        PeopleOut = 1
    }

    class RenderingSettings
    {
        public bool DrawBackground { get; set; }
        public bool DrawPixels { get; set; }
        public bool DrawSkeleton { get; set; }
        public bool PrintID { get; set; }
        public bool PrintState { get; set; }
    }

    class GeneratingSettings
    {
        public SkeletonProfile SkeletonProfile { get; set; }
        public Dictionary<KeyboardTrigger, ushort> KeyboardMapping { get; set; }
    }

    class DiagnosticsSettings
    {
        public TraceLevel TraceLevel { get; set; }
    }

    class UtilitySettings : ICloneable
    {
        public RenderingSettings RenderingSettings { get; set; }
        public GeneratingSettings GeneratingSettings { get; set; }
        public DiagnosticsSettings DiagnosticsSettings { get; set; }

        public object Clone()
        {
            UtilitySettings settings = new UtilitySettings();

            settings.RenderingSettings = new RenderingSettings();
            settings.RenderingSettings.DrawBackground = this.RenderingSettings.DrawBackground;
            settings.RenderingSettings.DrawPixels = this.RenderingSettings.DrawPixels;
            settings.RenderingSettings.DrawSkeleton = this.RenderingSettings.DrawSkeleton;
            settings.RenderingSettings.PrintID = this.RenderingSettings.PrintID;
            settings.RenderingSettings.PrintState = this.RenderingSettings.PrintState;

            settings.GeneratingSettings = new GeneratingSettings();
            settings.GeneratingSettings.SkeletonProfile = this.GeneratingSettings.SkeletonProfile;
            settings.GeneratingSettings.KeyboardMapping = new Dictionary<KeyboardTrigger, ushort>();
            foreach (KeyValuePair<KeyboardTrigger, ushort> kv in this.GeneratingSettings.KeyboardMapping)
            {
                settings.GeneratingSettings.KeyboardMapping.Add(kv.Key, kv.Value);
            }

            settings.DiagnosticsSettings = new DiagnosticsSettings();
            settings.DiagnosticsSettings.TraceLevel = this.DiagnosticsSettings.TraceLevel;

            return settings;
        }
    }

    interface IConfigurationPersist
    {
        UtilitySettings Load();
        void Persist(UtilitySettings settings);
    }

    class Configuration
    {
        public const string OPENNI_CONFIG_FILE = @"OpenNI.xml";
        public const int STATISTICS_TIMER_INTERVAL_MS = 1000;

        public static UtilitySettings Settings
        {
            get
            {
                if (settings == null)
                {
                    lock (settingsLock)
                    {
                        if (settings == null)
                        {
                            settings = GetPersistProvider().Load();
                        }
                    }
                }
                return settings;
            }

            set
            {
                lock (settingsLock)
                {
                    settings = value;
                    GetPersistProvider().Persist(settings);
                }
            }
        }

        private static IConfigurationPersist GetPersistProvider()
        {
            if (provider == null)
            {
                lock (providerLock)
                {
                    if (provider == null)
                    {
                        provider = new ConfigurationPersistProvider();
                    }
                }
            }
            return provider;
        }

        private static IConfigurationPersist provider = null;
        private static object providerLock = new object();
        private static UtilitySettings settings = null;
        private static object settingsLock = new object();
    }

    class ConfigurationPersistProvider : IConfigurationPersist
    {
        public ConfigurationPersistProvider()
        { }

        #region IDiagnostics

        public UtilitySettings Load()
        {
            try
            {
                System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);

                UtilitySettings settings = new UtilitySettings();

                settings.RenderingSettings = new RenderingSettings();
                settings.RenderingSettings.DrawBackground =
                    Boolean.Parse(config.AppSettings.Settings[SettingKeys.drawBackground.ToString()].Value);
                settings.RenderingSettings.DrawPixels =
                    Boolean.Parse(config.AppSettings.Settings[SettingKeys.drawPixels.ToString()].Value);
                settings.RenderingSettings.DrawSkeleton =
                    Boolean.Parse(config.AppSettings.Settings[SettingKeys.drawSkeleton.ToString()].Value);
                settings.RenderingSettings.PrintID =
                    Boolean.Parse(config.AppSettings.Settings[SettingKeys.printID.ToString()].Value);
                settings.RenderingSettings.PrintState =
                    Boolean.Parse(config.AppSettings.Settings[SettingKeys.printState.ToString()].Value);

                settings.GeneratingSettings = new GeneratingSettings();
                settings.GeneratingSettings.SkeletonProfile =
                    (SkeletonProfile)Enum.Parse(typeof(SkeletonProfile), config.AppSettings.Settings[SettingKeys.skeletonProfile.ToString()].Value, true);

                settings.GeneratingSettings.KeyboardMapping = new Dictionary<KeyboardTrigger, ushort>();
                settings.GeneratingSettings.KeyboardMapping.Add(
                    KeyboardTrigger.PeopleIn,
                    ushort.Parse(config.AppSettings.Settings[SettingKeys.peopleInKey.ToString()].Value));
                settings.GeneratingSettings.KeyboardMapping.Add(
                    KeyboardTrigger.PeopleOut,
                    ushort.Parse(config.AppSettings.Settings[SettingKeys.peopleOutKey.ToString()].Value));

                settings.DiagnosticsSettings = new DiagnosticsSettings();
                settings.DiagnosticsSettings.TraceLevel =
                    (TraceLevel)Enum.Parse(typeof(TraceLevel), config.AppSettings.Settings[SettingKeys.traceLevel.ToString()].Value, true);

                return settings;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Persist(UtilitySettings settings)
        {
            try
            {
                System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);

                config.AppSettings.Settings[SettingKeys.drawBackground.ToString()].Value =
                    settings.RenderingSettings.DrawBackground.ToString();
                config.AppSettings.Settings[SettingKeys.drawPixels.ToString()].Value =
                    settings.RenderingSettings.DrawPixels.ToString();
                config.AppSettings.Settings[SettingKeys.drawSkeleton.ToString()].Value =
                    settings.RenderingSettings.DrawSkeleton.ToString();
                config.AppSettings.Settings[SettingKeys.printID.ToString()].Value =
                    settings.RenderingSettings.DrawBackground.ToString();
                config.AppSettings.Settings[SettingKeys.printState.ToString()].Value =
                    settings.RenderingSettings.DrawBackground.ToString();

                config.AppSettings.Settings[SettingKeys.skeletonProfile.ToString()].Value =
                    settings.GeneratingSettings.SkeletonProfile.ToString();
                config.AppSettings.Settings[SettingKeys.peopleInKey.ToString()].Value =
                    settings.GeneratingSettings.KeyboardMapping[KeyboardTrigger.PeopleIn].ToString();
                config.AppSettings.Settings[SettingKeys.peopleOutKey.ToString()].Value =
                    settings.GeneratingSettings.KeyboardMapping[KeyboardTrigger.PeopleOut].ToString();

                config.AppSettings.Settings[SettingKeys.traceLevel.ToString()].Value =
                    settings.DiagnosticsSettings.TraceLevel.ToString();

                config.Save(System.Configuration.ConfigurationSaveMode.Modified);
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion IDiagnostics
    }
}