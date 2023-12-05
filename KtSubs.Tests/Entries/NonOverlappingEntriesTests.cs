using FluentAssertions;
using KtSubs.Core.Entries;
using KtSubs.Infrastructure.Services.EntryMergers;
using Xunit;

namespace KtSubs.InfrastructureTests.Entries
{
    public class NonOverlappingEntriesTests
    {
        [Fact]
        public void ShouldHaveCorrectlyCombinedEntries()
        {
            var entries = GetSampleEntries();

            var nonOverlappingEntries = new NonOverlappingEntries();
            var result = nonOverlappingEntries.Create(entries);

            result.Should().HaveCount(10);
        }

        [Fact]
        public void ValidateValues()
        {
            var entries = GetSampleEntries();

            var nonOverlappingEntries = new NonOverlappingEntries();
            var result = nonOverlappingEntries.Create(entries).ToList();

            result[0].AppearAt.Should().Be(TimeSpan.FromSeconds(0));
            result[0].DisappearAt.Should().Be(TimeSpan.FromSeconds(5));
            result[0].SelectMany(x => x.Words).ToList().Should().BeEquivalentTo(new List<string> { "Welcome" });

            result[3].AppearAt.Should().Be(TimeSpan.FromSeconds(12));
            result[3].DisappearAt.Should().Be(TimeSpan.FromSeconds(15));
            result[3].SelectMany(x => x.Words).ToList().Should().BeEquivalentTo(new List<string> { "DIRECTED", "BY", "to", "our", "world", "Town", "sign" });

            result[7].AppearAt.Should().Be(TimeSpan.FromSeconds(35));
            result[7].DisappearAt.Should().Be(TimeSpan.FromSeconds(37));
            result[7].SelectMany(x => x.Words).ToList().Should().BeEquivalentTo(new List<string> { "Hello", "Town", "sign", "John", "Doe" });
        }

        [Fact]
        public void SimpleCombineCase()
        {
            var entries = new List<Entry>
            {
                CreateEntry(0, 10, "Info", "Thanks for downloading our subtitles", 1),
                CreateEntry(5, 8, "Dialogue", "Hello", 2),
            };

            var nonOverlappingEntries = new NonOverlappingEntries();
            var result = nonOverlappingEntries.Create(entries).ToList();

            result.Should().BeEquivalentTo(new List<IEntry>
            {
                new Entry(
                    TimeSpan.FromSeconds(0),
                    TimeSpan.FromSeconds(5),
                    new EntryContent(1, "Info",
                        new List<string> { "Thanks", "for", "downloading", "our", "subtitles"})),
                new EntryGroup(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(8), new List<EntryContent>
                {
                    new EntryContent(1, "Info", new List<string> { "Thanks", "for", "downloading", "our", "subtitles"}),
                    new EntryContent(2, "Dialogue", new List<string> { "Hello"}),
                }),
                new Entry(
                    TimeSpan.FromSeconds(8),
                    TimeSpan.FromSeconds(10),
                    new EntryContent(1, "Info",
                        new List<string> { "Thanks", "for", "downloading", "our", "subtitles"}))
            });
        }

        private static int entryId = 1;

        private List<Entry> GetSampleEntries()
        {
            var entries = new List<Entry>
            {
                CreateEntry(10, 15, "INFO", "DIRECTED BY"),
                CreateEntry(20, 40, "INFO", "John Doe"),

                CreateEntry(12, 37, "BG_TRANSLATIONS", "Town sign"),

                CreateEntry(0, 5, "OP", "Welcome"),
                CreateEntry(7, 30, "OP", "to our world"),
                CreateEntry(35, 65, "OP", "Hello"),
            };
            return entries;
        }

        private Entry CreateEntry(int appearAtSeconds, int disappearAtSeconds, string layer, string content)
        {
            return new Entry(TimeSpan.FromSeconds(appearAtSeconds), TimeSpan.FromSeconds(disappearAtSeconds), new EntryContent(entryId++, layer, content.Split(' ').ToList()));
        }

        private Entry CreateEntry(int appearAtSeconds, int disappearAtSeconds, string layer, string content, int subtitleLineId)
        {
            return new Entry(TimeSpan.FromSeconds(appearAtSeconds), TimeSpan.FromSeconds(disappearAtSeconds), new EntryContent(subtitleLineId, layer, content.Split(' ').ToList()));
        }
    }
}