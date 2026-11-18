using System.Linq.Expressions;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Features.Exams.Commands.PublishExam;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Domain.Exceptions;

namespace StepByStepCoding.Security.Tests.Application.Features.Exams
{
    public sealed class PublishExamCommandHandlerTests : TestBase
    {
        public PublishExamCommandHandlerTests(ITestOutputHelper output) : base(output) { }

        #region Publishing rules

        [Theory]
        [InlineData(1, false, true)]
        [InlineData(0, false, false)]
        [InlineData(1, true, false)]
        public async Task Handle_RequiresQuestionsAndUnexpiredExam(int questionCount, bool expired, bool permitted)
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var scope = Fixture.Create<AcademicReadScope>();
            var exam = new Exam(Fixture.Create<Guid>(), Fixture.Create<string>(), null,
                now.AddHours(-2), expired ? now.AddMinutes(-1) : now.AddHours(1), 60);
            var request = Fixture.Build<PublishExamCommand>().With(command => command.Id, exam.Id)
                .With(command => command.Version, exam.Version).Create();
            var repository = new Mock<IExamRepository>();
            var questions = new Mock<IExamQuestionRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var access = new Mock<IAcademicAccess>();
            var clock = new Mock<TimeProvider>();
            clock.Setup(service => service.GetUtcNow()).Returns(now);
            access.Setup(service => service.CurrentScope()).Returns(scope);
            repository.Setup(service => service.GetById(exam.Id, It.IsAny<Expression<Func<Exam, object?>>[]>(), CancellationToken.None))
                .ReturnsAsync(exam);
            questions.Setup(service => service.Count(scope, exam.Id, CancellationToken.None)).ReturnsAsync(questionCount);
            var handler = new PublishExamCommandHandler(repository.Object, unitOfWork.Object, access.Object, clock.Object, questions.Object);

            WriteOutput("Arranged input", (nameof(request), request), (nameof(questionCount), questionCount), (nameof(expired), expired));

            // Act
            var result = await Record.ExceptionAsync(() => handler.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result), ("Status", exam.Status));

            // Assert
            if (permitted)
                result.Should().BeNull();
            else
                result.Should().BeOfType<BusinessRuleException>();

            exam.Status.Should().Be(permitted ? ExamStatus.Published : ExamStatus.Draft);
            unitOfWork.Verify(service => service.Commit(CancellationToken.None), permitted ? Times.Once() : Times.Never());
            access.Verify(service => service.ManageExam(exam), Times.Once);
        }

        #endregion
    }
}
