using KtSubs.Core.Entries;

namespace KtSubs.Core.Services
{
    public record ReadingResult(List<IEntry> Entries, HashSet<string> Layers);

    public interface ISubtitlesReader
    {
        ReadingResult GetEntries(string path);
    }
}