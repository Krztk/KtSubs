using FluentAssertions;
using KtSubs.Core.Selection;
using Xunit;

namespace KtSubs.CoreTests.Selections
{
    public class PatternTests
    {
        [Fact]
        public void ShouldConvertPatternIntoSingleSelections()
        {
            var pattern = new Pattern("1, 3, 8");
            var result = pattern.ToSelections(10);

            result.Should().BeEquivalentTo(new List<ISelection>() { new Selection(1), new Selection(3), new Selection(8) });
        }

        [Fact]
        public void ShouldConvertPatternIntoSingleSelectionsThatDoNotExceedMaxNumber()
        {
            var pattern = new Pattern("8, 10, 11, 12");
            var maxWordNumber = 10;

            var result = pattern.ToSelections(maxWordNumber);

            result.Should().BeEquivalentTo(new List<ISelection>() { new Selection(8), new Selection(10) });
        }

        [Fact]
        public void ShouldConvertMultipleSingleSelectionsPattern()
        {
            var pattern = new Pattern("1-3*");
            var maxWordNumber = 10;

            var result = pattern.ToSelections(maxWordNumber);

            result.Should().BeEquivalentTo(new List<ISelection>() { new Selection(1), new Selection(2), new Selection(3) });
        }

        [Fact]
        public void ShouldConvertMultipleSingleSelectionsPatternWithOverlaps()
        {
            var pattern = new Pattern("1-3*, 2-4*, 2");
            var maxWordNumber = 10;

            var result = pattern.ToSelections(maxWordNumber);

            result.Should().BeEquivalentTo(new List<ISelection>() { new Selection(1), new Selection(2), new Selection(3), new Selection(4) });
        }

        [Fact]
        public void ShouldConvertPatternIntoMergedSelection()
        {
            var pattern = new Pattern("1-5");
            var maxWordNumber = 10;
            var result = pattern.ToSelections(maxWordNumber);

            result.Should().BeEquivalentTo(new List<ISelection>() { new MergedSelection(1, 5) });
        }

        [Fact]
        public void ShouldConvertPatternIntoSelectionsThatDoNotExceedMaxNumber()
        {
            var pattern = new Pattern("8-12, 9, 10, 11");
            var maxWordNumber = 10;
            var result = pattern.ToSelections(maxWordNumber);

            result.Should().BeEquivalentTo(new List<ISelection>() { new MergedSelection(8, 10), new Selection(9), new Selection(10) });
        }

        [Fact]
        public void ShouldNotConvertIntoSelectionIfNumberIsSmallerThan1()
        {
            var pattern = new Pattern("0");
            var maxWordNumber = 10;
            var result = pattern.ToSelections(maxWordNumber);

            result.Should().BeEmpty();
        }

        [Fact]
        public void ShouldConvertToMergedSelectionEvenIfLeftNumberIsEqualTo0()
        {
            var pattern = new Pattern("0-10");
            var maxWordNumber = 10;
            var result = pattern.ToSelections(maxWordNumber);

            result.Should().BeEquivalentTo(new List<ISelection> { new MergedSelection(1, 10) });
        }

        [Fact]
        public void ShouldConvertMergedSelectionPatternIntoSingleSelectionIfLeftAndRightAreEqual()
        {
            var pattern = new Pattern("5-5");
            var maxWordNumber = 10;

            var result = pattern.ToSelections(maxWordNumber);

            result.Should().BeEquivalentTo(new List<ISelection> { new Selection(5) });
        }

        [Fact]
        public void ShouldNotConvertMergedSelectionPatternIfLeftIsGreaterThanRight()
        {
            var pattern = new Pattern("6-5");
            var maxWordNumber = 10;
            var result = pattern.ToSelections(maxWordNumber);

            result.Should().BeEmpty();
        }

        [Fact]
        public void ShouldNotConvertSelectionPatternIfNumberIsNegative()
        {
            var pattern = new Pattern("-5");
            var maxWordNumber = 10;
            var result = pattern.ToSelections(maxWordNumber);

            result.Should().BeEmpty();
        }
    }
}