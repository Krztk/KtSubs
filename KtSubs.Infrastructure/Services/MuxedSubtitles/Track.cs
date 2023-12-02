namespace KtSubs.Infrastructure.Services.MuxedSubtitles
{
    public class Track
    {
        public string Codec { get; set; }
        public int Id { get; set; }
        public Properties Properties { get; set; }
        public string Type { get; set; }

        public override string ToString()
        {
            return $"Track ID: {Id} - {Codec}, {Type}, ({Properties.Language})";
        }
    }
}