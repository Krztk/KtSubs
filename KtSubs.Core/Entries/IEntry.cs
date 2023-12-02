namespace KtSubs.Core.Entries
{
    public interface IEntry : IEnumerable<EntryContent>
    {
        public TimeSpan AppearAt { get; }
        public TimeSpan DisappearAt { get; }

        public bool ContainsOneOf(IEnumerable<string> layers);
    }
}