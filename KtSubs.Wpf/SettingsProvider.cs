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
                Hotkey = Hotkey.FromString(appSettings.SelectionActivatorHotkey, Hotkey.Default()),
                PauseVideoWhenSelecting = appSettings.PauseVideoWhenSelecting,
                WebInterfacePassword = appSettings.WebInterfacePassword,
                Port = appSettings.Port,
                DisplaySelectionWindowWhenSubtitleEntryIsInRange = appSettings.DisplaySelectionWindowWhenSubtitleEntryIsInRange,
                MkvToolnixFolder = appSettings.MkvToolnixFolder,
                LocationOfExtractedSubtitles = appSettings.LocationOfExtractedSubtitles,
            };
        }

        public void SaveSettings(Settings settings)
        {
            var appSettings = Properties.AppSettings.Default;
            appSettings.SelectionActivatorHotkey = settings.Hotkey.ToString();
            appSettings.WebInterfacePassword = settings.WebInterfacePassword;
            appSettings.PauseVideoWhenSelecting = settings.PauseVideoWhenSelecting;
            appSettings.Port = settings.Port;
            appSettings.DisplaySelectionWindowWhenSubtitleEntryIsInRange = settings.DisplaySelectionWindowWhenSubtitleEntryIsInRange;
            appSettings.MkvToolnixFolder = settings.MkvToolnixFolder;
            appSettings.LocationOfExtractedSubtitles = settings.LocationOfExtractedSubtitles;
            appSettings.Save();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}