using KtSubs.Core.Entries;

namespace KtSubs.Infrastructure.Services.EntryMergers
{
    public class NonOverlappingEntries
    {
        public IEnumerable<IEntry> Create(IEnumerable<Entry> entries)
        {
            var intervals = entries
                .SelectMany(e => ToEnumerable(e.AppearAt, e.DisappearAt))
                .OrderBy(x => x)
                .Distinct()
                .ToIntervals();

            foreach (var interval in intervals)
            {
                var overlappingEntries = entries.Where(e => interval.Intersects(e.AppearAt, e.DisappearAt)).ToList();

                if (overlappingEntries.Count == 0)
                    continue;

                if (overlappingEntries.Count == 1)
                {
                    yield return new Entry(interval.Start, interval.End, overlappingEntries[0].Content);
                }
                else
                {
                    yield return new EntryGroup(interval.Start, interval.End, overlappingEntries.Select(entry => entry.Content).ToList());
                }
            }
        }

        private static IEnumerable<T> ToEnumerable<T>(params T[] items) => items;
    }
}