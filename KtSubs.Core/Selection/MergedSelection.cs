using KtSubs.Infrastructure.Entries;
using System.Text;

namespace KtSubs.Core.Selection
{
    public sealed class MergedSelection : ISelection
    {
        private readonly int left, right;

        public MergedSelection(int left, int right)
        {
            if (left <= 0) throw new ArgumentOutOfRangeException(nameof(left));
            if (left >= right) throw new ArgumentOutOfRangeException(nameof(right));

            this.left = left;
            this.right = right;
        }

        public IEnumerable<int> SelectedWordNumbers => Enumerable.Range(left, right - left + 1);

        public override bool Equals(object? obj)
        {
            return obj is MergedSelection selection &&
                   left == selection.left &&
                   right == selection.right;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(left, right);
        }

        public string GetPattern()
        {
            return $"{left}-{right}";
        }

        public string GetSelectedValue(IList<string> input)
        {
            if (right - left + 1 > input.Count)
                throw new IndexOutOfRangeException(nameof(right));

            return String.Join(' ', input.Skip(left - 1).Take(right - left + 1));
        }

        public string GetSelectedValue(DisplayEntry displayEntry)
        {
            var startIndex = left - 1;
            var numberOfElements = right - left + 1;
            var result = new StringBuilder();
            var whitespace = "";
            var i = 0;
            foreach (var entry in displayEntry.Entries)
            {
                foreach (var content in entry.Value)
                {
                    foreach (var word in content.Words)
                    {
                        if (i >= startIndex)
                        {
                            var words = content.Words;
                            result.Append($"{whitespace}{word}");
                            whitespace = " ";
                            numberOfElements--;
                            if (numberOfElements == 0)
                                return result.ToString();
                        }
                        i++;
                    }
                }
            }
            return result.ToString();
        }

        public ISelection GetSelectionIncreasedBy(int offset)
        {
            return new MergedSelection(left + offset, right + offset);
        }

        public static bool operator ==(MergedSelection? left, MergedSelection? right)
        {
            return EqualityComparer<MergedSelection>.Default.Equals(left, right);
        }

        public static bool operator !=(MergedSelection? left, MergedSelection? right)
        {
            return !(left == right);
        }
    }
}