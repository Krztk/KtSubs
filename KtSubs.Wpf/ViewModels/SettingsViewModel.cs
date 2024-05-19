using CommunityToolkit.Mvvm.Input;
using KtSubs.Core.Settings;
using KtSubs.Wpf.Services;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace KtSubs.Wpf.ViewModels
{
    public class SettingsViewModel : Dialog
    {
        private readonly ISettingsProvider settingsProvider;
        private readonly HotkeyManager hotkeyManager;

        public Settings Settings { get; private set; }
        public ICommand PreviewKeyDownCommand { get; }
        public IRelayCommand SaveCommand { get; private set; }

        private string hotkeyString;
        public string HotkeyString { get => hotkeyString; set => SetProperty(ref hotkeyString, value); }

        private ModifierKeys allowedModifiers = ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt;

        public SettingsViewModel(ISettingsProvider settingsProvider, HotkeyManager hotkeyManager)
        {
            Settings = settingsProvider.GetSettings();
            HotkeyString = ConvertHotkeyToHumanReadableString(Settings.Hotkey);
            SaveCommand = new RelayCommand(HandleSettingSave);
            this.settingsProvider = settingsProvider;
            this.hotkeyManager = hotkeyManager;
            PreviewKeyDownCommand = new RelayCommand<KeyEventArgs>(OnPreviewKeyDown);
        }

        private void OnPreviewKeyDown(KeyEventArgs? e)
        {
            var modifiers = allowedModifiers & Keyboard.Modifiers;
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            var hotkey = new Hotkey((int)key, (int)modifiers);
            HotkeyString = ConvertHotkeyToHumanReadableString(hotkey);
            Settings.Hotkey = hotkey;
            e.Handled = true;
        }

        private void HandleSettingSave()
        {
            Debug.WriteLine("HandleSettingSave");
            settingsProvider.SaveSettings(Settings);
            Close(true);
        }

        public override void OnOpen()
        {
            hotkeyManager.UnregisterHotKey();
        }

        public override void OnClose(bool dialogResult)
        {
            Debug.WriteLine("OnClose, dialogResult? {0}", dialogResult);
            Debug.WriteLine("Settings.Hotkey {0}", Settings.Hotkey);

            var hotkey = dialogResult ? Settings.Hotkey : hotkeyManager.CurrentHotkey;
            hotkeyManager.RegisterHotKey(hotkey);
        }

        private string ConvertHotkeyToHumanReadableString(Hotkey hotkey)
        {
            if (hotkey == null) return String.Empty;

            var modifiers = ((ModifierKeys)hotkey.Modifiers).ToString().Replace(", ", " + ");
            var key = ((Key)hotkey.Key).ToString();

            return hotkey.Modifiers != 0 ?
                $"{modifiers} + {key}"
                : key.ToString();
        }
    }
}