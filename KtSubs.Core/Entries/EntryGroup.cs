using System.Collections;

namespace KtSubs.Core.Entries
{
    public class EntryGroup : IEntry
    {
        public TimeSpan AppearAt { get; set; }

        public TimeSpan DisappearAt { get; set; }

        public List<EntryContent> EntryContent { get; } = new();

        public EntryGroup()
        {
        }

        public EntryGroup(TimeSpan appearAt, TimeSpan disappearAt, List<EntryContent> entryContent)
        {
            AppearAt = appearAt;
            DisappearAt = disappearAt;
            this.EntryContent = entryContent;
        }

        public IEnumerator<EntryContent> GetEnumerator()
        {
            foreach (var content in EntryContent)
            {
                yield return content;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsOneOf(IEnumerable<string> layers)
        {
            foreach (var entry in EntryContent)
            {
                if (layers.Contains(entry.Layer))
                    return true;
            }

            return false;
        }
    }
}