using FluentAssertions;
using KtSubs.Core.Selection;
using Xunit;

namespace KtSubs.CoreTests.Selections
{
    public class PatternToSelectionConverterTests
    {
        [Fact]
        public void ShouldConvertPatternIntoSingleSelections()
        {
            //arrange
            var selectionConverter = new PatternToSelectionConverter();
            var pattern = "1, 3, 8";
            //act
            var result = selectionConverter.Convert(pattern, 10);

            //assert
            result.Should().BeEquivalentTo(new List<ISelection>() { new SingleSelection(1), new SingleSelection(3), new SingleSelection(8) });
        }

        [Fact]
        public void ShouldConvertPatternIntoSingleSelectionsThatDoNotExceedMaxNumber()
        {
            //arrange
            var selectionConverter = new PatternToSelectionConverter();
            var pattern = "8, 10, 11, 12";
            var maxWordNumber = 10;
            //act
            var result = selectionConverter.Convert(pattern, maxWordNumber);

            //assert
            result.Should().BeEquivalentTo(new List<ISelection>() { new SingleSelection(8), new SingleSelection(10) });
        }

        [Fact]
        public void ShouldConvertMultipleSingleSelectionsPattern()
        {
            var selectionConverter = new PatternToSelectionConverter();
            var pattern = "1-3*";
            var maxWordNumber = 10;
            //act
            var result = selectionConverter.Convert(pattern, maxWordNumber);

            //assert
            result.Should().BeEquivalentTo(new List<ISelection>() { new SingleSelection(1), new SingleSelection(2), new SingleSelection(3) });
        }

        [Fact]
        public void ShouldConvertPatternIntoMergedSelection()
        {
            //arrange
            var selectionConverter = new PatternToSelectionConverter();
            var pattern = "1-5";
            var maxWordNumber = 10;
            //act
            var result = selectionConverter.Convert(pattern, maxWordNumber);

            //assert
            result.Should().BeEquivalentTo(new List<ISelection>() { new MergedSelection(1, 5) });
        }

        [Fact]
        public void ShouldConvertPatternIntoSelectionsThatDoNotExceedMaxNumber()
        {
            //arrange
            var selectionConverter = new PatternToSelectionConverter();
            var pattern = "8-12, 9, 10, 11";
            var maxWordNumber = 10;
            //act
            var result = selectionConverter.Convert(pattern, maxWordNumber);

            //assert
            result.Should().BeEquivalentTo(new List<ISelection>() { new MergedSelection(8, 10), new SingleSelection(9), new SingleSelection(10) });
        }

        [Fact]
        public void ShouldNotConvertIntoSelectionIfNumberIsSmallerThan1()
        {
            //arrange
            var selectionConverter = new PatternToSelectionConverter();
            var pattern = "0";
            var maxWordNumber = 10;
            //act
            var result = selectionConverter.Convert(pattern, maxWordNumber);

            //assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ShouldConvertToMergedSelectionEvenIfLeftNumberIsEqualTo0()
        {
            //arrange
            var selectionConverter = new PatternToSelectionConverter();
            var pattern = "0-10";
            var maxWordNumber = 10;
            //act
            var result = selectionConverter.Convert(pattern, maxWordNumber);

            //assert
            result.Should().BeEquivalentTo(new List<ISelection> { new MergedSelection(1, 10) });
        }

        [Fact]
        public void ShouldConvertMergedSelectionPatternIntoSingleSelectionIfLeftAndRightAreEqual()
        {
            //arrange
            var selectionConverter = new PatternToSelectionConverter();
            var pattern = "5-5";
            var maxWordNumber = 10;
            //act
            var result = selectionConverter.Convert(pattern, maxWordNumber);

            //assert
            result.Should().BeEquivalentTo(new List<ISelection> { new SingleSelection(5) });
        }

        [Fact]
        public void ShouldNotConvertMergedSelectionPatternIfLeftIsGreaterThanRight()
        {
            //arrange
            var selectionConverter = new PatternToSelectionConverter();
            var pattern = "6-5";
            var maxWordNumber = 10;
            //act
            var result = selectionConverter.Convert(pattern, maxWordNumber);

            //assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ShouldNotConvertSelectionPatternIfNumberIsNegative()
        {
            //arrange
            var selectionConverter = new PatternToSelectionConverter();
            var pattern = "-5";
            var maxWordNumber = 10;
            //act
            var result = selectionConverter.Convert(pattern, maxWordNumber);

            //assert
            result.Should().BeEmpty();
        }
    }
}