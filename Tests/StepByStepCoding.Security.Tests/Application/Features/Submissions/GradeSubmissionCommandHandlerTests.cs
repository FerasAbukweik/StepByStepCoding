using System.Linq.Expressions;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Features.Submissions.Commands.GradeSubmission;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Domain.Exceptions;

namespace StepByStepCoding.Security.Tests.Application.Features.Submissions
{
    public sealed class GradeSubmissionCommandHandlerTests : TestBase
    {
        public GradeSubmissionCommandHandlerTests(ITestOutputHelper output) : base(output) { }

        #region Grading

        [Theory]
        [InlineData(9)]
        [InlineData(11)]
        public async Task Handle_GradesOnlyWithinQuestionMaximum(int score)
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var scope = Fixture.Create<AcademicReadScope>();
            var question = new ExamQuestion(Fixture.Create<Guid>(), Fixture.Create<string>(), Fixture.Create<string>(), null, 10, 0);
            var submission = new Submission(Fixture.Create<Guid>(), question.Id, Fixture.Create<string>(), ProgrammingLanguage.Python, now);
            var request = Fixture.Build<GradeSubmissionCommand>().With(command => command.Id, submission.Id)
                .With(command => command.Version, submission.Version).With(command => command.Score, score).Create();
            var repository = new Mock<ISubmissionRepository>();
            var questions = new Mock<IExamQuestionRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var access = new Mock<IAcademicAccess>();
            var clock = new Mock<TimeProvider>();
            access.Setup(service => service.CurrentScope()).Returns(scope);
            clock.Setup(service => service.GetUtcNow()).Returns(now.AddMinutes(1));
            repository.Setup(service => service.GetById(submission.Id,
                It.IsAny<Expression<Func<Submission, object?>>[]>(), CancellationToken.None)).ReturnsAsync(submission);
            questions.Setup(service => service.GetById(question.Id,
                It.IsAny<Expression<Func<ExamQuestion, object?>>[]>(), CancellationToken.None)).ReturnsAsync(question);
            var handler = new GradeSubmissionCommandHandler(repository.Object, questions.Object, unitOfWork.Object, access.Object, clock.Object);

            WriteOutput("Arranged input", (nameof(request), request), ("MaxScore", question.MaxScore));

            // Act
            var result = await Record.ExceptionAsync(() => handler.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result), (nameof(submission), submission));

            // Assert
            if (score <= 10)
            {
                result.Should().BeNull();
                submission.Score.Should().Be(score);
                submission.ReviewedBy.Should().Be(scope.UserId);
                submission.Feedback.Should().Be(request.Feedback);
            }
            else
            {
                result.Should().BeOfType<BusinessRuleException>();
                submission.Score.Should().BeNull();
            }

            repository.Verify(service => service.Update(submission), score <= 10 ? Times.Once() : Times.Never());
            unitOfWork.Verify(service => service.Commit(CancellationToken.None), score <= 10 ? Times.Once() : Times.Never());
        }

        #endregion
    }
}
