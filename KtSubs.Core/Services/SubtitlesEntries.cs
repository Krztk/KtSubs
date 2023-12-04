using KtSubs.Core.Entries;
using LanguageExt;

namespace KtSubs.Core.Services
{
    public class SubtitlesEntries
    {
        private readonly ISubtitlesStore subtitlesStore;
        private readonly LayersSettings layersSettings;

        private IReadOnlyList<IEntry> SubtitleEntries => subtitlesStore.Entries;

        public int EntriesCount => subtitlesStore.Entries.Count;

        public SubtitlesEntries(ISubtitlesStore subtitlesStore, LayersSettings layerSettings)
        {
            this.subtitlesStore = subtitlesStore;
            this.layersSettings = layerSettings;
        }

        public EntriesResult? GetEntry(TimeSpan timeStamp)
        {
            var timeSpanComparer = Comparer<TimeSpan>.Default;

            for (int index = 0; index < SubtitleEntries.Count; index++)
            {
                var entry = SubtitleEntries[index];

                int appearAtComparison = timeSpanComparer.Compare(entry.AppearAt, timeStamp);
                if (appearAtComparison <= 0
                    && timeSpanComparer.Compare(entry.DisappearAt, timeStamp) > 0)
                {
                    var result = entry.ToList();
                    return new EntriesResult(index, entry.AppearAt, entry.DisappearAt, result);
                }

                if (appearAtComparison > 0 || index == SubtitleEntries.Count - 1)
                {
                    return PickEntryThatIsCloserToTimeStamp(index, timeStamp);
                }
            }

            return null;
        }

        private EntriesResult PickEntryThatIsCloserToTimeStamp(int index, TimeSpan timeStamp)
        {
            var prevEntry = SubtitleEntries.ElementAtOrDefault(index - 1);
            var currentEntry = SubtitleEntries[index];

            if (prevEntry == null)
            {
                return new EntriesResult(index, currentEntry.AppearAt, currentEntry.DisappearAt, currentEntry.ToList());
            }

            TimeSpan timeBetweenPreviousEntryDisappearAndTimeStamp = timeStamp - prevEntry.DisappearAt;
            TimeSpan timeBetweenTimeStampAndEntry = currentEntry.AppearAt - timeStamp;

            if (timeBetweenPreviousEntryDisappearAndTimeStamp < timeBetweenTimeStampAndEntry)
            {
                return new EntriesResult(index - 1, prevEntry.AppearAt, prevEntry.DisappearAt, prevEntry.ToList());
            }

            return new EntriesResult(index, currentEntry.AppearAt, currentEntry.DisappearAt, currentEntry.ToList());
        }

        public EntriesResult? GetEntry(int index)
        {
            if (index < 0 || index >= SubtitleEntries.Count)
                return null;

            var entry = SubtitleEntries[index];
            var result = entry.ToList();
            return new EntriesResult(index, entry.AppearAt, entry.DisappearAt, result);
        }

        public EntriesResult? GetClosestEntryToRight(int index)
        {
            if (index < 0 || index >= SubtitleEntries.Count)
                return null;

            var tempIndex = index;
            var activeLayers = layersSettings.LayerNameIsActivePair
                                    .Select(x => x.Value)
                                    .Filter(x => x.IsActive).Select(x => x.Name)
                                    .ToList();

            while (tempIndex < SubtitleEntries.Count)
            {
                var entry = SubtitleEntries[tempIndex];
                if (entry.ContainsOneOf(activeLayers))
                {
                    return new EntriesResult(tempIndex, entry.AppearAt, entry.DisappearAt, entry.ToList());
                }
                tempIndex++;
            }

            return null;
        }

        public EntriesResult? GetClosestEntryToLeft(int index)
        {
            if (index < 0 || index >= SubtitleEntries.Count)
                return null;

            var tempIndex = index;
            var activeLayers = layersSettings.LayerNameIsActivePair
                                    .Select(x => x.Value)
                                    .Filter(x => x.IsActive).Select(x => x.Name)
                                    .ToList();

            while (tempIndex >= 0)
            {
                var entry = SubtitleEntries[tempIndex];
                if (entry.ContainsOneOf(activeLayers))
                {
                    return new EntriesResult(tempIndex, entry.AppearAt, entry.DisappearAt, entry.ToList());
                }
                tempIndex--;
            }

            return null;
        }

        public IEnumerable<EntryContent> GetEntries(int leftIndex, int rightIndex)
        {
            var entries = SubtitleEntries.Skip(leftIndex).Take(rightIndex - leftIndex + 1);
            return entries.SelectMany(x => x);
        }
    }

    public record EntriesResult(int Index, TimeSpan Appear, TimeSpan Disappear, List<EntryContent> GroupOfEntryContent);
}