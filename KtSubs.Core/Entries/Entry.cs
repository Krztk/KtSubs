using System.Collections;

namespace KtSubs.Core.Entries
{
    public class Entry : IEntry
    {
        public TimeSpan AppearAt { get; }
        public TimeSpan DisappearAt { get; }
        public EntryContent Content { get; }

        public Entry(TimeSpan appearAt, TimeSpan disappearAt, EntryContent content)
        {
            AppearAt = appearAt;
            DisappearAt = disappearAt;
            Content = content;
        }

        public IEnumerator<EntryContent> GetEnumerator()
        {
            yield return Content;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsOneOf(IEnumerable<string> layers)
        {
            return layers.Contains(Content.Layer);
        }
    }
}