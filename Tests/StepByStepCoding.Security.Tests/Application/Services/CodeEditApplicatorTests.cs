using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Realtime;
using StepByStepCoding.Application.Services;

namespace StepByStepCoding.Security.Tests.Application.Services
{
    public sealed class CodeEditApplicatorTests : TestBase
    {
        public CodeEditApplicatorTests(ITestOutputHelper output) : base(output) { }

        #region Valid edits

        [Fact]
        public void Apply_WithMultipleOffsets_AppliesEditsFromRightToLeft()
        {
            // Arrange
            const string content = "abcdef";
            CodeChangeSet[] changes = [new() { Changes =
                [new CodeEdit { RangeOffset = 1, RangeLength = 1, Text = "X" },
                 new CodeEdit { RangeOffset = 4, RangeLength = 1, Text = "Y" }] }];

            WriteOutput("Arranged input", (nameof(content), content), (nameof(changes), changes));

            // Act
            var result = CodeEditApplicator.Apply(content, changes);

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().Be("aXcdYf");
        }

        [Fact]
        public void Apply_WithSequentialEvents_UsesEachPreviousResult()
        {
            // Arrange
            var text = Fixture.Create<string>();
            CodeChangeSet[] changes =
            [
                new() { Changes = [new CodeEdit { RangeOffset = 0, RangeLength = 0, Text = text }] },
                new() { Changes = [new CodeEdit { RangeOffset = text.Length, RangeLength = 0, Text = "!" }] }
            ];

            WriteOutput("Arranged input", (nameof(text), text), (nameof(changes), changes));

            // Act
            var result = CodeEditApplicator.Apply("", changes);

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().Be(text + "!");
        }

        #endregion

        #region Invalid edits

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(4, 0)]
        [InlineData(1, 3)]
        public void Apply_WithOutOfBoundsRange_Throws(int offset, int length)
        {
            // Arrange
            CodeChangeSet[] changes = [new() { Changes =
                [new CodeEdit { RangeOffset = offset, RangeLength = length, Text = Fixture.Create<string>() }] }];

            WriteOutput("Arranged input", (nameof(changes), changes));

            // Act
            var result = Record.Exception(() => CodeEditApplicator.Apply("abc", changes));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeOfType<CustomValidationException>();
        }

        [Fact]
        public void Apply_WithSplitSurrogatePair_Throws()
        {
            // Arrange
            CodeChangeSet[] changes = [new() { Changes = [new CodeEdit { RangeOffset = 1, RangeLength = 0, Text = "x" }] }];

            WriteOutput("Arranged input", (nameof(changes), changes));

            // Act
            var result = Record.Exception(() => CodeEditApplicator.Apply("😀", changes));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeOfType<CustomValidationException>();
        }

        #endregion
    }
}
