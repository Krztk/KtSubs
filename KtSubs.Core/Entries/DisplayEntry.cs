using KtSubs.Core.Entries;
using System.Text;

namespace KtSubs.Infrastructure.Entries
{
    public class DisplayEntry
    {
        public SortedDictionary<string, List<EntryContent>> Entries { get; } = new();
        public bool HasMultipleContentLines => Entries.Count > 1;

        public int NumberOfWords => Entries.SelectMany(x => x.Value)
                                           .Sum(x => x.Words.Count);

        public DisplayEntry()
        {
        }

        public DisplayEntry(EntryContent content)
        {
            IncludeRight(content);
        }

        public DisplayEntry(IEnumerable<EntryContent> groupOfContent)
        {
            foreach (var content in groupOfContent)
            {
                IncludeRight(content);
            }
        }

        public bool IncludeRight(EntryContent content)
        {
            var layer = content.Layer;
            if (Entries.ContainsKey(layer))
            {
                if (Entries[layer].FindIndex(e => e.Id == content.Id) != -1)
                    return false;

                Entries[layer].Add(content);
            }
            else
            {
                Entries.Add(layer, new List<EntryContent> { content });
            }

            return true;
        }

        public bool IncludeLeft(EntryContent content)
        {
            var layer = content.Layer;
            if (Entries.ContainsKey(layer))
            {
                if (Entries[layer].FindIndex(e => e.Id == content.Id) != -1)
                    return false;

                Entries[layer].Insert(0, content);
            }
            else
            {
                Entries.Add(layer, new List<EntryContent> { content });
            }

            return true;
        }

        public IncludeResult IncludeRight(IEnumerable<EntryContent> elements)
        {
            var allLayers = new SortedSet<string>(Entries.Keys);
            var nameAndNumberOfAddedWordsPair = new Dictionary<string, int>();
            var atLeastOneEntryContentIncluded = false;
            foreach (var element in elements)
            {
                var isIncluded = IncludeRight(element);
                if (isIncluded)
                {
                    atLeastOneEntryContentIncluded = true;
                    allLayers.Add(element.Layer);
                    if (nameAndNumberOfAddedWordsPair.ContainsKey(element.Layer))
                    {
                        nameAndNumberOfAddedWordsPair[element.Layer] += element.Words.Count;
                    }
                    else
                    {
                        nameAndNumberOfAddedWordsPair.Add(element.Layer, element.Words.Count);
                    }
                }
            }

            IncludeResult result = new(atLeastOneEntryContentIncluded);
            var offset = 0;
            foreach (var layer in allLayers)
            {
                result.AddLayerWithNumberOfIncludedWords(layer, offset);

                if (nameAndNumberOfAddedWordsPair.ContainsKey(layer))
                    offset += nameAndNumberOfAddedWordsPair[layer];
            }

            return result;
        }

        public IncludeResult IncludeLeft(IEnumerable<EntryContent> elements)
        {
            var allLayers = new SortedSet<string>(Entries.Keys);

            var nameAndNumberOfAddedWordsPair = new Dictionary<string, int>();
            var atLeastOneEntryContentIncluded = false;
            foreach (var element in elements)
            {
                var isIncluded = IncludeLeft(element);
                if (isIncluded)
                {
                    atLeastOneEntryContentIncluded = true;
                    allLayers.Add(element.Layer);
                    if (nameAndNumberOfAddedWordsPair.ContainsKey(element.Layer))
                    {
                        nameAndNumberOfAddedWordsPair[element.Layer] += element.Words.Count;
                    }
                    else
                    {
                        nameAndNumberOfAddedWordsPair.Add(element.Layer, element.Words.Count);
                    }
                }
            }

            IncludeResult result = new(atLeastOneEntryContentIncluded);
            var offset = 0;
            foreach (var layer in allLayers)
            {
                if (nameAndNumberOfAddedWordsPair.ContainsKey(layer))
                    offset += nameAndNumberOfAddedWordsPair[layer];

                result.AddLayerWithNumberOfIncludedWords(layer, offset);
            }

            return result;
        }

        public string? GetLayerNameAtWordIndex(int index)
        {
            var enumeratedIndexCount = 0;
            foreach (var entry in Entries)
            {
                foreach (var e in entry.Value)
                {
                    enumeratedIndexCount += e.Words.Count;
                    if (index < enumeratedIndexCount)
                        return entry.Key;
                }
            }

            return null;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var entry in Entries)
            {
                sb.AppendLine($"{entry.Key}:");
                foreach (var content in entry.Value)
                {
                    sb.Append(String.Join(' ', content.Words));
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}