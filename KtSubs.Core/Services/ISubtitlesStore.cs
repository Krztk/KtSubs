using KtSubs.Core.Entries;

namespace KtSubs.Core.Services
{
    public interface ISubtitlesStore
    {
        event EventHandler? SubtitlesLoaded;

        IReadOnlyList<IEntry> Entries { get; }
        IReadOnlySet<string> Layers { get; }

        void Load(string path);
    }
}