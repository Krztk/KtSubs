using KtSubs.Core.Services;

namespace KtSubs.Infrastructure.Services.Readers
{
    public class ReaderFactory
    {
        public ISubtitlesReader CreateReader(SubtitlesType type)
        {
            return type switch
            {
                SubtitlesType.Srt => new SrtReader(),
                SubtitlesType.Ssa => new SsaReader(),
                _ => throw new NotSupportedException(nameof(type)),
            };
        }
    }
}