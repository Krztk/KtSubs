using KtSubs.Infrastructure.Entries;

namespace KtSubs.Core.Selection
{
    public interface ISelection
    {
        string GetSelectedValue(IList<string> input);

        string GetSelectedValue(DisplayEntry displayEntry);

        ISelection GetSelectionIncreasedBy(int offset);

        public string GetPattern();

        IEnumerable<int> SelectedWordNumbers { get; }
    }
}