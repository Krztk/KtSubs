using Newtonsoft.Json;

namespace KtSubs.Infrastructure.Services.MuxedSubtitles
{
    public class Properties
    {
        [JsonProperty("codec_id")]
        public string CodecId { get; set; }

        public string Encoding { get; set; }
        public string Language { get; set; }
        public int Number { get; set; }
    }
}