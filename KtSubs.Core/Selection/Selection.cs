using KtSubs.Core.Extensions;
using KtSubs.Infrastructure.Entries;

namespace KtSubs.Core.Selection
{
    public sealed class Selection : ISelection
    {
        private readonly int number;
        private readonly int index;

        public Selection(int number)
        {
            if (number < 1)
                throw new ArgumentOutOfRangeException(nameof(number));
            this.number = number;
            index = number - 1;
        }

        public IEnumerable<int> SelectedWordNumbers => new int[] { number };

        public override bool Equals(object? obj)
        {
            return obj is Selection selection &&
                   number == selection.number;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(number);
        }

        public string GetPattern()
        {
            return number.ToString();
        }

        public string GetSelectedValue(IList<string> input)
        {
            if (number - 1 >= input.Count)
                throw new IndexOutOfRangeException(nameof(number));

            return input[number - 1].RemovePunctuationCharactersFromEnd();
        }

        public string GetSelectedValue(DisplayEntry displayEntry)
        {
            int totalCountOfEnumeratedLists = 0;
            foreach (var entry in displayEntry.Entries)
            {
                foreach (var content in entry.Value)
                {
                    var words = content.Words;
                    totalCountOfEnumeratedLists += words.Count;
                    if (index < totalCountOfEnumeratedLists)
                    {
                        var i = index - (totalCountOfEnumeratedLists - words.Count);

                        return words[i].RemovePunctuationCharactersFromEnd();
                    }
                }
            }
            throw new IndexOutOfRangeException("Something went wrong.");
        }

        public ISelection GetSelectionIncreasedBy(int offset)
        {
            return new Selection(number + offset);
        }

        public static bool operator ==(Selection? left, Selection? right)
        {
            return EqualityComparer<Selection>.Default.Equals(left, right);
        }

        public static bool operator !=(Selection? left, Selection? right)
        {
            return !(left == right);
        }
    }
}