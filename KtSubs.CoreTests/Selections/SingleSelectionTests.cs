using FluentAssertions;
using KtSubs.Core.Entries;
using KtSubs.Core.Selection;
using KtSubs.Infrastructure.Entries;
using Xunit;

namespace KtSubs.CoreTests.Selections
{
    public class SingleSelectionTests
    {
        private readonly DisplayEntry displayEntry;

        public SingleSelectionTests()
        {
            displayEntry = new DisplayEntry(new EntryContent(1, "DEFAULT", new List<string> { "This", "is", "not", "a", "drill." }));
        }

        [Theory]
        [InlineData(1, "This")]
        [InlineData(2, "is")]
        public void ShouldSelectGivenWords(int wordNumber, string expectedSelectionResult)
        {
            //arrange
            ISelection selection = new SingleSelection(wordNumber);

            //act
            var result = selection.GetSelectedValue(displayEntry);

            //assert
            result.Should().BeEquivalentTo(expectedSelectionResult);
        }

        [Fact]
        public void ShouldSelectWordWithoutPunctuationCharacters()
        {
            //arrange
            ISelection selection = new SingleSelection(5);

            //act
            var result = selection.GetSelectedValue(displayEntry);

            //assert
            result.Should().BeEquivalentTo("drill");
        }

        [Fact]
        public void ShouldThrowIndexOutOfRangeException()
        {
            //arrange
            ISelection selection = new SingleSelection(6);

            //act
            var sut = () => selection.GetSelectedValue(displayEntry);

            //assert
            sut.Should().Throw<IndexOutOfRangeException>();
        }
    }
}