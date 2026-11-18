using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Features.Courses.Commands.CreateCourse;
using StepByStepCoding.Application.Features.Courses.Commands.DeleteCourse;
using StepByStepCoding.Application.Features.Courses.Commands.UpdateCourse;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Security.Tests.Application.Features.Courses
{
    public sealed class CourseCommandHandlerTests : TestBase
    {
        public CourseCommandHandlerTests(ITestOutputHelper output) : base(output) { }

        #region Create

        [Fact]
        public async Task Create_WithValidRequest_AddsCourseAndCommits()
        {
            // Arrange
            var request = Fixture.Create<CreateCourseCommand>();
            var repository = new Mock<ICourseRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            Course? added = null;
            repository.Setup(service => service.Add(It.IsAny<Course>()))
                .Callback<Course>(entity => added = entity).ReturnsAsync((Course entity) => entity);
            var handler = new CreateCourseCommandHandler(repository.Object, unitOfWork.Object);

            WriteOutput("Arranged input", (nameof(request), request));

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            WriteOutput("Act result", (nameof(result), result), (nameof(added), added));

            // Assert
            added.Should().NotBeNull();
            added!.Name.Should().Be(request.Name.Trim());
            result.Should().Be(added.Id);
            repository.Verify(service => service.Add(added), Times.Once);
            unitOfWork.Verify(service => service.Commit(CancellationToken.None), Times.Once);
            unitOfWork.Verify(service => service.Rollback(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Create_WhenPersistenceFails_RollsBackAndPreservesException(bool commitFails)
        {
            // Arrange
            var request = Fixture.Create<CreateCourseCommand>();
            var repository = new Mock<ICourseRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var failure = new InvalidOperationException(Fixture.Create<string>());
            repository.Setup(service => service.Add(It.IsAny<Course>())).ReturnsAsync((Course entity) => entity);
            if (commitFails)
                unitOfWork.Setup(service => service.Commit(CancellationToken.None)).ThrowsAsync(failure);
            else
                repository.Setup(service => service.Add(It.IsAny<Course>())).ThrowsAsync(failure);

            var handler = new CreateCourseCommandHandler(repository.Object, unitOfWork.Object);

            WriteOutput("Arranged input", (nameof(request), request), (nameof(commitFails), commitFails));

            // Act
            var result = await Record.ExceptionAsync(() => handler.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeSameAs(failure);
            unitOfWork.Verify(service => service.Rollback(CancellationToken.None), Times.Once);
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_WithCurrentVersion_UpdatesCourseAndCommits()
        {
            // Arrange
            var course = new Course(Fixture.Create<string>());
            var request = Fixture.Build<UpdateCourseCommand>()
                .With(command => command.Id, course.Id).With(command => command.Version, course.Version).Create();
            var repository = new Mock<ICourseRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            repository.Setup(service => service.GetById(course.Id)).ReturnsAsync(course);
            var handler = new UpdateCourseCommandHandler(repository.Object, unitOfWork.Object);

            WriteOutput("Arranged input", (nameof(request), request));

            // Act
            var result = await Record.ExceptionAsync(() => handler.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result), (nameof(course), course));

            // Assert
            result.Should().BeNull();
            course.Name.Should().Be(request.Name.Trim());
            repository.Verify(service => service.Update(course), Times.Once);
            unitOfWork.Verify(service => service.Commit(CancellationToken.None), Times.Once);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Update_WithMissingCourseOrStaleVersion_DoesNotWrite(bool exists)
        {
            // Arrange
            var course = new Course(Fixture.Create<string>());
            var request = Fixture.Build<UpdateCourseCommand>().With(command => command.Id, course.Id).Create();
            var repository = new Mock<ICourseRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            repository.Setup(service => service.GetById(request.Id)).ReturnsAsync(exists ? course : null);
            var handler = new UpdateCourseCommandHandler(repository.Object, unitOfWork.Object);

            WriteOutput("Arranged input", (nameof(request), request), (nameof(exists), exists));

            // Act
            var result = await Record.ExceptionAsync(() => handler.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            if (exists)
                result.Should().BeOfType<ConflictException>();
            else
                result.Should().BeOfType<NotFoundException>();

            repository.Verify(service => service.Update(It.IsAny<Course>()), Times.Never);
            unitOfWork.Verify(service => service.Commit(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Delete

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Delete_RespectsDependentRecords(bool hasDependents)
        {
            // Arrange
            var course = new Course(Fixture.Create<string>());
            var request = Fixture.Build<DeleteCourseCommand>()
                .With(command => command.Id, course.Id).With(command => command.Version, course.Version).Create();
            var repository = new Mock<ICourseRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            repository.Setup(service => service.GetById(course.Id)).ReturnsAsync(course);
            repository.Setup(service => service.HasDependents(course.Id, CancellationToken.None)).ReturnsAsync(hasDependents);
            var handler = new DeleteCourseCommandHandler(repository.Object, unitOfWork.Object);

            WriteOutput("Arranged input", (nameof(request), request), (nameof(hasDependents), hasDependents));

            // Act
            var result = await Record.ExceptionAsync(() => handler.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            if (hasDependents)
                result.Should().BeOfType<ConflictException>();
            else
                result.Should().BeNull();

            repository.Verify(service => service.Delete(course), hasDependents ? Times.Never() : Times.Once());
            unitOfWork.Verify(service => service.Commit(CancellationToken.None), hasDependents ? Times.Never() : Times.Once());
        }

        #endregion
    }
}
