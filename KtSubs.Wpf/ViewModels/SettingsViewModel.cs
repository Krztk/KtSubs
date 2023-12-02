using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KtSubs.Core.Settings;

namespace KtSubs.Wpf.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsProvider settingsProvider;

        public Settings Settings { get; private set; }
        public IRelayCommand SaveCommand { get; private set; }

        public SettingsViewModel(ISettingsProvider settingsProvider)
        {
            Settings = settingsProvider.GetSettings();
            SaveCommand = new RelayCommand(HandleSettingSave);
            this.settingsProvider = settingsProvider;
        }

        private void HandleSettingSave()
        {
            settingsProvider.SaveSettings(Settings);
        }
    }
}