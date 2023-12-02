using KtSubs.Core.Entries;
using KtSubs.Core.Services;
using System;
using System.Collections.Generic;

namespace KtSubs.WpfTests.ViewModels.SelectionViewModelTests
{
    public class TestStore : ISubtitlesStore
    {
        public IReadOnlySet<string> Layers { get; } = new HashSet<string> { "DEFAULT" };

        public IReadOnlyList<IEntry> Entries { get; } = new List<IEntry>()
             {
                 new Entry(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), new EntryContent(1, "DEFAULT", new List<string> {"First", "word!"})),
                 new Entry(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30), new EntryContent(2, "DEFAULT", new List<string> {"Second", "Word"})),
                 new Entry(TimeSpan.FromSeconds(35), TimeSpan.FromSeconds(55), new EntryContent(3, "DEFAULT", new List<string> {"Third", "The", "end"})),
             };

        public event EventHandler? SubtitlesLoaded;

        public void Load(string path)
        {
            throw new NotImplementedException();
        }
    }
}