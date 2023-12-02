using KtSubs.Core.Entries;
using KtSubs.Core.Extensions;
using KtSubs.Core.Services;
using KtSubs.Infrastructure.Services.EntryMergers;
using System.Text.RegularExpressions;

namespace KtSubs.Infrastructure.Services.Readers
{
    public class SsaReader : ISubtitlesReader
    {
        private Regex splitRegex = new(@"(?:\s|\\N|\\n)+", RegexOptions.Compiled);
        private Regex overrideSsaCodesRegex = new(@"\{.*?}", RegexOptions.Compiled);

        public ReadingResult GetEntries(string path)
        {
            var layerNames = new HashSet<string>();
            int id = 0;
            var entries = new List<Entry>();
            var lines = File.ReadAllLines(path);
            var startProcessing = false;
            var formatTextIndex = -1;
            var start = 0;
            var end = 0;
            var style = 0;
            var lastEventIndex = 0;

            foreach (var line in lines)
            {
                if (line == "[Events]")
                {
                    startProcessing = true;
                    continue;
                }

                if (!startProcessing)
                    continue;

                if (formatTextIndex == -1)
                {
                    var tempLine = line.Replace("Format:", "");
                    tempLine = tempLine.Trim();
                    var events = tempLine.Split(',').Select(item => item.Trim()).ToList();
                    lastEventIndex = events.Count - 1;
                    start = events.IndexOf("Start");
                    end = events.IndexOf("End");
                    style = events.IndexOf("Style");
                    formatTextIndex = events.IndexOf("Text");
                    continue;
                }

                if (!line.StartsWith("Dialogue"))
                    continue;

                var startIndex = line.IndexOf(':') + 1;
                var values = line.GetSubstringsAtIndexes(lastEventIndex, new List<int> { start, end, formatTextIndex, style }, ',', startIndex);
                var appearAt = GetTimeStamp(values[0]);
                var disappearAt = GetTimeStamp(values[1]);

                var dialogue = overrideSsaCodesRegex.Replace(values[2].Trim(), string.Empty);
                var styleName = values[3].Trim();
                layerNames.Add(styleName);
                var contentWords = splitRegex.Split(dialogue).ToList();
                var content = new EntryContent(id, styleName, contentWords);
                entries.Add(new Entry(appearAt, disappearAt, content));
                id++;
            }

            var combiner = new EntryMerger();

            return new ReadingResult(combiner.Merge(entries).ToList(), layerNames);
        }

        private static TimeSpan GetTimeStamp(string timeString)
        {
            var values = timeString.Split(new char[] { ':', '.' }).Select(value => value.Trim()).ToList();
            if (values.Count != 4)
                throw new Exception($"Wrong timestamp format, {timeString}");

            if (!int.TryParse(values[0], out int hours))
                throw new Exception($"Wrong timestamp format, cannot parse hours string: {values[0]}");

            if (!int.TryParse(values[1], out int mins))
                throw new Exception($"Wrong timestamp format, cannot parse hours string: {values[1]}");

            if (!int.TryParse(values[2], out int secs))
                throw new Exception($"Wrong timestamp format, cannot parse hours string: {values[2]}");

            if (!int.TryParse(values[3], out int hundredths))
                throw new Exception($"Wrong timestamp format, cannot parse hours string: {values[3]}");

            return new TimeSpan(0, hours, mins, secs, hundredths * 10);
        }
    }
}