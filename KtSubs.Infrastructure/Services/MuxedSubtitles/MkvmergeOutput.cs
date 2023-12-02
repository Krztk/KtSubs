namespace KtSubs.Infrastructure.Services.MuxedSubtitles
{
    public class MkvmergeOutput
    {
        public object[] Errors { get; set; }
        public Track[] Tracks { get; set; }
        public object[] Warnings { get; set; }
    }
}