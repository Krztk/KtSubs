using KtSubs.Core.Settings;
using System;

namespace KtSubs.Wpf
{
    public class SettingsProvider : ISettingsProvider
    {
        public event EventHandler? SettingsChanged;

        public Settings GetSettings()
        {
            var appSettings = Properties.AppSettings.Default;

            return new Settings
            {
                PauseVideoWhenSelecting = appSettings.PauseVideoWhenSelecting,
                WebInterfacePassword = appSettings.WebInterfacePassword,
                Port = appSettings.Port,
                DisplaySelectionWindowWhenSubtitleEntryIsInRange = appSettings.DisplaySelectionWindowWhenSubtitleEntryIsInRange,
                MkvToolnixFolder = appSettings.MkvToolnixFolder
            };
        }

        public void SaveSettings(Settings settings)
        {
            var appSettings = Properties.AppSettings.Default;
            appSettings.WebInterfacePassword = settings.WebInterfacePassword;
            appSettings.PauseVideoWhenSelecting = settings.PauseVideoWhenSelecting;
            appSettings.Port = settings.Port;
            appSettings.DisplaySelectionWindowWhenSubtitleEntryIsInRange = settings.DisplaySelectionWindowWhenSubtitleEntryIsInRange;
            appSettings.MkvToolnixFolder = settings.MkvToolnixFolder;
            appSettings.Save();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}