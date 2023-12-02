namespace KtSubs.Core.Settings
{
    public class Settings
    {
        public string WebInterfacePassword { get; set; }
        public int Port { get; set; }
        public bool PauseVideoWhenSelecting { get; set; }
        public bool DisplaySelectionWindowWhenSubtitleEntryIsInRange { get; set; }
        public string MkvToolnixFolder { get; set; }
    }
}