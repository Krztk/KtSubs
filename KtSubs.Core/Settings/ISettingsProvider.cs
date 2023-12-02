namespace KtSubs.Core.Settings
{
    public interface ISettingsProvider
    {
        event EventHandler SettingsChanged;

        Settings GetSettings();

        void SaveSettings(Settings settings);
    }
}