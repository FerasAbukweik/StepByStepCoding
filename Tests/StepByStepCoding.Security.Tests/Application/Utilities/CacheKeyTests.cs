using StepByStepCoding.Application.Features.Courses.Queries.GetCoursesList;
using StepByStepCoding.Application.Features.Files.Commands.UpdateLectureFile;
using StepByStepCoding.Application.Features.Files.Queries.GetFileDetail;
using StepByStepCoding.Application.Utilities.Caching;

namespace StepByStepCoding.Security.Tests.Application.Utilities
{
    public sealed class CacheKeyTests : TestBase
    {
        public CacheKeyTests(ITestOutputHelper output) : base(output) { }

        #region Shared file keys

        [Fact]
        public void Key_FileQueryAndUpdate_UseSameFileId()
        {
            // Arrange
            var id = Fixture.Create<Guid>();
            var query = Fixture.Build<GetFileDetailQuery>().With(request => request.Id, id).Create();
            var command = Fixture.Build<UpdateLectureFileCommand>().With(request => request.Id, id).Create();

            WriteOutput("Arranged input", (nameof(query), query), (nameof(command), command));

            // Act
            var queryKey = query.Key();
            var updateKey = command.Key();

            WriteOutput("Act result", (nameof(queryKey), queryKey), (nameof(updateKey), updateKey));

            // Assert
            queryKey.Should().Be($"{CacheGroups.File}:{id:D}");
            updateKey.Should().Be(queryKey);
        }

        #endregion

        #region Serialized request keys

        [Fact]
        public void Key_DifferentPages_HaveDifferentKeysWithSamePrefix()
        {
            // Arrange
            var first = new GetCoursesListQuery { Page = 1, RecordsPerPage = 10 };
            var second = new GetCoursesListQuery { Page = 2, RecordsPerPage = 10 };

            WriteOutput("Arranged input", (nameof(first), first), (nameof(second), second));

            // Act
            var firstKey = first.Key();
            var secondKey = second.Key();

            WriteOutput("Act result", (nameof(firstKey), firstKey), (nameof(secondKey), secondKey));

            // Assert
            firstKey.Should().NotBe(secondKey);
            firstKey.Should().StartWith(first.Prefix + ":");
            secondKey.Should().StartWith(first.Prefix + ":");
            firstKey.Should().NotContain("\"Prefix\"").And.NotContain("\"Key\"");
        }

        #endregion
    }
}
