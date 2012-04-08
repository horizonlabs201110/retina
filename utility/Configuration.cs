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
        keyboardPeopleIn = 6,
        keyboardPeopleOut = 7
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
        public Dictionary<KeyboardTrigger, int> KeyboardMapping { get; set; }
    }

    class UtilitySettings
    {
        public RenderingSettings RenderingSettings { get; set; }
        public GeneratingSettings GeneratingSettings { get; set; }
    }

    interface IConfigurationPersist
    {
        UtilitySettings Load();
        void Persist(UtilitySettings settings);
    }

    class Configuration
    {
        public const string OPENNI_CONFIG_FILE = @"OpenNI.xml";
        public const int STATISTICS_INTERVAL_MS = 1000;

        public static UtilitySettings Settings
        {
            get
            {
                if (settings == null)
                {
                    lock (settingsLock)
                    {
                        settings = GetPersistProvider().Load();
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

            settings.GeneratingSettings.KeyboardMapping = new Dictionary<KeyboardTrigger, int>();
            settings.GeneratingSettings.KeyboardMapping.Add(
                KeyboardTrigger.PeopleIn,
                int.Parse(config.AppSettings.Settings[SettingKeys.keyboardPeopleIn.ToString()].Value));
            settings.GeneratingSettings.KeyboardMapping.Add(
                KeyboardTrigger.PeopleOut,
                int.Parse(config.AppSettings.Settings[SettingKeys.keyboardPeopleOut.ToString()].Value));


            return settings;
        }

        public void Persist(UtilitySettings settings)
        {
            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
            
            config.AppSettings.Settings.Add(
                SettingKeys.drawBackground.ToString(),
                settings.RenderingSettings.DrawBackground.ToString());
            config.AppSettings.Settings.Add(
                SettingKeys.drawPixels.ToString(),
                settings.RenderingSettings.DrawPixels.ToString());
            config.AppSettings.Settings.Add(
                SettingKeys.drawSkeleton.ToString(),
                settings.RenderingSettings.DrawSkeleton.ToString());
            config.AppSettings.Settings.Add(
                SettingKeys.printID.ToString(),
                settings.RenderingSettings.DrawBackground.ToString());
            config.AppSettings.Settings.Add(
                SettingKeys.printState.ToString(),
                settings.RenderingSettings.DrawBackground.ToString());


            config.AppSettings.Settings.Add(
                SettingKeys.skeletonProfile.ToString(),
                settings.GeneratingSettings.SkeletonProfile.ToString());
            config.AppSettings.Settings.Add(
                SettingKeys.keyboardPeopleIn.ToString(),
                settings.GeneratingSettings.KeyboardMapping[KeyboardTrigger.PeopleIn].ToString());
            config.AppSettings.Settings.Add(
                SettingKeys.keyboardPeopleOut.ToString(),
                settings.GeneratingSettings.KeyboardMapping[KeyboardTrigger.PeopleOut].ToString());

            config.Save(System.Configuration.ConfigurationSaveMode.Modified);
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");
        }

        #endregion IDiagnostics
    }
}
