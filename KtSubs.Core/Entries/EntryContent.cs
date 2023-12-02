namespace KtSubs.Core.Entries
{
    public class EntryContent
    {
        public int Id { get; }
        public string Layer { get; }
        public List<string> Words { get; }

        public EntryContent(int id, string layer, List<string> words)
        {
            Id = id;
            Layer = layer;
            Words = words;
        }
    }
}