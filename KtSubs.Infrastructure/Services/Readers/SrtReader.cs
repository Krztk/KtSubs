using KtSubs.Core.Entries;
using KtSubs.Core.Exceptions;
using KtSubs.Core.Services;
using System.Text.RegularExpressions;

namespace KtSubs.Infrastructure.Services.Readers
{
    internal enum LineType
    {
        Number,
        TimeStamp,
        Content,
    }

    public class SrtReader : ISubtitlesReader
    {
        private LineType state = LineType.Number;
        private List<string> entryWords = new();
        private string lastTimeStampLine = string.Empty;
        private List<IEntry> entries = new();
        private int id = 1;
        private const string DEFAULT_LAYER_NAME = "DEFAULT";

        public ReadingResult GetEntries(string path)
        {
            Reset();
            using (var sr = new StreamReader(path, true))
            {
                string? line;
                int lineNumber = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    lineNumber++;
                    try
                    {
                        HandleState(line);
                    }
                    catch (WrongTimestampFormatException ex)
                    {
                        throw new SubtitlesReadingException($"Invalid or unexpected content at line: {lineNumber}", ex);
                    }
                }
                if (state == LineType.Content && entryWords.Count > 0)
                {
                    entries.Add(GetSubtitleEntry());
                }
            }

            return new ReadingResult(entries, new HashSet<string> { DEFAULT_LAYER_NAME });
        }

        private void Reset()
        {
            id = 1;
            entries = new List<IEntry>();
            lastTimeStampLine = string.Empty;
            state = LineType.Number;
        }

        private void HandleState(string line)
        {
            if (state == LineType.Number)
            {
                state = LineType.TimeStamp;
            }
            else if (state == LineType.TimeStamp)
            {
                lastTimeStampLine = line;
                state = LineType.Content;
            }
            else if (state == LineType.Content)
            {
                if (IsBlankLine(line))
                {
                    state = LineType.Number;
                    entries.Add(GetSubtitleEntry());
                    entryWords = new();
                }
                else
                {
                    AddContent(line);
                }
            }
        }

        private void AddContent(string line)
        {
            line = RemoveTextFormatting(line);
            foreach (var word in line.Trim().Split())
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    entryWords.Add(word);
                }
            }
        }

        private static string RemoveTextFormatting(string line)
        {
            line = line.Replace("<i>", "").Replace("</i>", "");
            line = line.Replace("<b>", "").Replace("</b>", "");
            line = Regex.Replace(line, "<font.*?>", "").Replace("</font>", "");
            return line;
        }

        private Entry GetSubtitleEntry()
        {
            var timeStamps = GetStartAndStopTimeStamp(lastTimeStampLine);
            var content = new EntryContent(id++, DEFAULT_LAYER_NAME, entryWords);
            return new Entry(timeStamps.Item1, timeStamps.Item2, content);
        }

        private static bool IsBlankLine(string line)
        {
            return line.Length == 0;
        }

        public static (TimeSpan, TimeSpan) GetStartAndStopTimeStamp(string line)
        {
            //00:05:00,400 --> 00:05:15,300
            var splited = line.Split("-->");
            var appearAt = GetTimeStamp(splited[0].Trim());
            var disappearAt = GetTimeStamp(splited[1].Trim());
            return (appearAt, disappearAt);
        }

        private static TimeSpan GetTimeStamp(string str)
        {
            var delimiters = new char[] { ':', ':', ',' };
            var values = new int[4];

            for (int i = 0; i < delimiters.Length; i++)
            {
                var delimiterIndex = str.IndexOf(delimiters[i]);
                if (delimiterIndex == -1)
                    throw new WrongTimestampFormatException();

                if (!int.TryParse(str.Substring(0, delimiterIndex), out int value))
                {
                    throw new WrongTimestampFormatException();
                }

                values[i] = value;
                str = str.Substring(delimiterIndex + 1);
            }

            if (!int.TryParse(str, out int lastValue))
            {
                throw new WrongTimestampFormatException();
            }
            values[3] = lastValue;

            return new TimeSpan(0, values[0], values[1], values[2], values[3]);
        }
    }
}