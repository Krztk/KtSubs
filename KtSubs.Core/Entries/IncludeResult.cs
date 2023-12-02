namespace KtSubs.Core.Entries
{
    public class IncludeResult
    {
        private Dictionary<string, int> layerNameAndNumberOfWordsIncludedPair = new Dictionary<string, int>();

        public int this[string i] => layerNameAndNumberOfWordsIncludedPair[i];

        public bool HasIncluded { get; }

        public IncludeResult(bool hasIncluded)
        {
            HasIncluded = hasIncluded;
        }

        public void AddLayerWithNumberOfIncludedWords(string layer, int numberOfWords)
        {
            layerNameAndNumberOfWordsIncludedPair.Add(layer, numberOfWords);
        }
    }
}