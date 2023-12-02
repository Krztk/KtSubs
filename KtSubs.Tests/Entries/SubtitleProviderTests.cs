using FluentAssertions;
using KtSubs.Core.Entries;
using KtSubs.Core.Services;
using Xunit;

namespace KtSubs.InfrastructureTests.Entries
{
    public class SubtitleProviderTests
    {
        private SubtitlesEntryFinder subtitlesProvider;

        public SubtitleProviderTests()
        {
            var store = new TestStore();
            subtitlesProvider = new SubtitlesEntryFinder(store, new LayersSettingsManager(store));
        }

        [Fact]
        public void ShouldSelectValueThatOverlapsEntry()
        {
            var result = subtitlesProvider.GetEntry(TimeSpan.FromSeconds(22));

            result.Should().NotBeNull();
            result.Appear.Should().Be(TimeSpan.FromSeconds(20));
            result.Disappear.Should().Be(TimeSpan.FromSeconds(30));
        }

        [Fact]
        public void ShouldGetEntryClosestEntryToTimestamp()
        {
            var result = subtitlesProvider.GetEntry(TimeSpan.FromSeconds(32));

            result.Should().NotBeNull();
            result.Appear.Should().Be(TimeSpan.FromSeconds(20));
            result.Disappear.Should().Be(TimeSpan.FromSeconds(30));
        }

        [Fact]
        public void ShouldGetEntryClosestEntryToTimestamp2()
        {
            var result = subtitlesProvider.GetEntry(TimeSpan.FromSeconds(34));

            result.Should().NotBeNull();
            result.Appear.Should().Be(TimeSpan.FromSeconds(35));
            result.Disappear.Should().Be(TimeSpan.FromSeconds(55));
        }

        [Fact]
        public void ShouldGetEntryClosestEntryToTimestamp3()
        {
            var result = subtitlesProvider.GetEntry(TimeSpan.FromSeconds(60));

            result.Should().NotBeNull();
            result.Appear.Should().Be(TimeSpan.FromSeconds(35));
            result.Disappear.Should().Be(TimeSpan.FromSeconds(55));
        }

        [Fact]
        public void ShouldGetEntryClosestEntryToTimestamp4()
        {
            var result = subtitlesProvider.GetEntry(TimeSpan.FromSeconds(0));

            result.Should().NotBeNull();
            result.Appear.Should().Be(TimeSpan.FromSeconds(5));
            result.Disappear.Should().Be(TimeSpan.FromSeconds(10));
        }
    }

    public class TestStore : ISubtitlesStore
    {
        public IReadOnlyList<IEntry> Entries { get; } = new List<IEntry>()
             {
                 new Entry(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), new EntryContent(1, "DEFAULT", new List<string> {"First"})),
                 new Entry(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30), new EntryContent(2, "DEFAULT", new List<string> {"Second"})),
                 new Entry(TimeSpan.FromSeconds(35), TimeSpan.FromSeconds(55), new EntryContent(3, "DEFAULT", new List<string> {"Third"})),
             };

        public IReadOnlySet<string> Layers { get; } = new HashSet<string> { "DEFAULT" };

        public event EventHandler? SubtitlesLoaded;

        public void Load(string path)
        {
            throw new NotImplementedException();
        }
    }
}