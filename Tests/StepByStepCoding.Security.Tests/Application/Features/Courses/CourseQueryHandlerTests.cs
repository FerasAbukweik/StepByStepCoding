using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Features.Courses.Queries.GetCourseDetail;
using StepByStepCoding.Application.Features.Courses.Queries.GetCoursesList;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Security.Tests.Application.Features.Courses
{
    public sealed class CourseQueryHandlerTests : TestBase
    {
        public CourseQueryHandlerTests(ITestOutputHelper output) : base(output) { }

        #region Details

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetDetail_HandlesExistingAndMissingCourses(bool exists)
        {
            // Arrange
            var course = new Course(Fixture.Create<string>());
            var request = Fixture.Build<GetCourseDetailQuery>().With(query => query.Id, course.Id).Create();
            var repository = new Mock<ICourseRepository>();
            var access = new Mock<IAcademicAccess>();
            repository.Setup(service => service.GetById(course.Id)).ReturnsAsync(exists ? course : null);
            var handler = new GetCourseDetailQueryHandler(repository.Object, access.Object);
            CourseDetailDTO? result = null;

            WriteOutput("Arranged input", (nameof(request), request), (nameof(exists), exists));

            // Act
            var exception = await Record.ExceptionAsync(async () => result = await handler.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result), (nameof(exception), exception));

            // Assert
            if (exists)
            {
                exception.Should().BeNull();
                result.Should().BeEquivalentTo(new { course.Id, course.Name, course.Version });
            }
            else
                exception.Should().BeOfType<NotFoundException>();

            repository.Verify(service => service.GetById(course.Id), Times.Once);
            access.Verify(service => service.CurrentScope(), Times.Once);
        }

        #endregion

        #region Pagination

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public async Task GetList_ReturnsRequestedPageAndTotalCount(int count)
        {
            // Arrange
            var scope = Fixture.Create<AcademicReadScope>();
            var request = Fixture.Build<GetCoursesListQuery>().With(query => query.Page, 2)
                .With(query => query.RecordsPerPage, 10).Create();
            var courses = Fixture.CreateMany<string>(count).Select(name => new Course(name)).ToArray();
            var page = new PaginatedDTO<Course> { Items = courses, TotalCount = count == 0 ? 0 : 25 };
            var repository = new Mock<ICourseRepository>();
            var access = new Mock<IAcademicAccess>();
            access.Setup(service => service.CurrentScope()).Returns(scope);
            repository.Setup(service => service.GetPage(scope, null, 2, 10, CancellationToken.None)).ReturnsAsync(page);
            var handler = new GetCoursesListQueryHandler(repository.Object, access.Object);

            WriteOutput("Arranged input", (nameof(request), request), (nameof(page), page));

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.TotalCount.Should().Be(page.TotalCount);
            result.Items.Select(item => item.Id).Should().Equal(courses.Select(course => course.Id));
            repository.Verify(service => service.GetPage(scope, null, 2, 10, CancellationToken.None), Times.Once);
            repository.VerifyNoOtherCalls();
        }

        #endregion
    }
}
