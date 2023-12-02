using FluentAssertions;
using KtSubs.Core.Entries;
using KtSubs.Core.Selection;
using KtSubs.Infrastructure.Entries;
using Xunit;

namespace KtSubs.CoreTests.Selections
{
    public class RangeSelectionTests
    {
        private readonly DisplayEntry displayEntry;

        public RangeSelectionTests()
        {
            displayEntry = new DisplayEntry(new EntryContent(1, "DEFAULT", new List<string> { "This", "is", "not", "a", "drill." }));
        }

        [Fact]
        public void ShouldSelectGivenWords()
        {
            //arrange
            ISelection selection = new MergedSelection(1, 5);

            //act
            var result = selection.GetSelectedValue(displayEntry);

            //assert
            result.Should().BeEquivalentTo("This is not a drill.");
        }
    }
}