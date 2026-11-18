using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Domain.Exceptions;

namespace StepByStepCoding.Security.Tests.Domain
{
    public sealed class AcademicEntityTests : TestBase
    {
        public AcademicEntityTests(ITestOutputHelper output) : base(output) { }

        #region Courses

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Course_WithMissingName_Throws(string? name)
        {
            // Arrange
            WriteOutput("Arranged input", (nameof(name), name));

            // Act
            var result = Record.Exception(() => new Course(name!));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeOfType<BusinessRuleException>();
        }

        [Fact]
        public void Course_Update_TrimsNameAndChangesVersion()
        {
            // Arrange
            var course = new Course(Fixture.Create<string>());
            var version = course.Version;
            var name = Fixture.Create<string>();

            WriteOutput("Arranged input", (nameof(name), name), (nameof(version), version));

            // Act
            course.Update($" {name} ");

            WriteOutput("Act result", (nameof(course), course));

            // Assert
            course.Name.Should().Be(name);
            course.Version.Should().NotBe(version);
            course.Id.Version.Should().Be(7);
        }

        #endregion

        #region Lectures

        [Fact]
        public void Lecture_StartAndEnd_ChangesStatusAndStoresUtcDates()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));
            var lecture = new Lecture(Fixture.Create<Guid>(), Fixture.Create<string>(), now);

            WriteOutput("Arranged input", (nameof(now), now), (nameof(lecture), lecture));

            // Act
            lecture.Start(now);
            lecture.End(now.AddHours(1));

            WriteOutput("Act result", (nameof(lecture), lecture));

            // Assert
            lecture.Status.Should().Be(LectureStatus.Ended);
            lecture.StartedAt.Should().Be(now);
            lecture.EndedAt.Should().Be(now.AddHours(1));
            lecture.StartedAt!.Value.Offset.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void Lecture_EndBeforeStart_Throws()
        {
            // Arrange
            var lecture = new Lecture(Fixture.Create<Guid>(), Fixture.Create<string>(), DateTimeOffset.UtcNow);
            var now = DateTimeOffset.UtcNow;

            WriteOutput("Arranged input", (nameof(lecture), lecture), (nameof(now), now));

            // Act
            var result = Record.Exception(() => lecture.End(now));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeOfType<BusinessRuleException>();
            lecture.Status.Should().Be(LectureStatus.Scheduled);
        }

        #endregion

        #region Exams and attempts

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1441)]
        public void Exam_WithInvalidDuration_Throws(int duration)
        {
            // Arrange
            var id = Fixture.Create<Guid>();
            var title = Fixture.Create<string>();
            var now = DateTimeOffset.UtcNow;

            WriteOutput("Arranged input", (nameof(id), id), (nameof(title), title), (nameof(duration), duration));

            // Act
            var result = Record.Exception(() => new Exam(id, title, null, now, now.AddHours(2), duration));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeOfType<BusinessRuleException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Exam_Publish_RequiresQuestions(int questionCount)
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var exam = new Exam(Fixture.Create<Guid>(), Fixture.Create<string>(), null, now, now.AddHours(2), 60);

            WriteOutput("Arranged input", (nameof(questionCount), questionCount), (nameof(exam), exam));

            // Act
            var result = Record.Exception(() => exam.Publish(questionCount));

            WriteOutput("Act result", (nameof(result), result), (nameof(exam), exam));

            // Assert
            if (questionCount == 0)
                result.Should().BeOfType<BusinessRuleException>();
            else
                result.Should().BeNull();

            exam.Status.Should().Be(questionCount == 0 ? ExamStatus.Draft : ExamStatus.Published);
        }

        [Theory]
        [InlineData(30, AttemptStatus.Submitted)]
        [InlineData(60, AttemptStatus.Expired)]
        public void Attempt_Finish_UsesItsExpiration(int minutes, AttemptStatus expected)
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var attempt = new ExamAttempt(Fixture.Create<Guid>(), Fixture.Create<Guid>(), now, now.AddMinutes(60));

            WriteOutput("Arranged input", (nameof(attempt), attempt), (nameof(minutes), minutes));

            // Act
            attempt.Finish(now.AddMinutes(minutes));

            WriteOutput("Act result", (nameof(attempt), attempt));

            // Assert
            attempt.Status.Should().Be(expected);
        }

        #endregion

        #region Submissions and files

        [Theory]
        [InlineData(-1)]
        [InlineData(11)]
        public void Submission_Grade_RejectsMarksOutsideQuestionRange(int score)
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var reviewer = Fixture.Create<Guid>();
            var submission = new Submission(Fixture.Create<Guid>(), Fixture.Create<Guid>(), Fixture.Create<string>(), ProgrammingLanguage.Python, now);

            WriteOutput("Arranged input", (nameof(score), score), (nameof(submission), submission));

            // Act
            var result = Record.Exception(() => submission.Grade(score, 10, null, reviewer, now));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeOfType<BusinessRuleException>();
            submission.Score.Should().BeNull();
        }

        [Fact]
        public void Submission_Grade_StoresMarkFeedbackAndReviewer()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var reviewer = Fixture.Create<Guid>();
            var feedback = Fixture.Create<string>();
            var submission = new Submission(Fixture.Create<Guid>(), Fixture.Create<Guid>(), Fixture.Create<string>(), ProgrammingLanguage.Python, now);

            WriteOutput("Arranged input", (nameof(reviewer), reviewer), (nameof(feedback), feedback));

            // Act
            submission.Grade(9.5m, 10, feedback, reviewer, now.AddMinutes(1));

            WriteOutput("Act result", (nameof(submission), submission));

            // Assert
            submission.Score.Should().Be(9.5m);
            submission.Feedback.Should().Be(feedback);
            submission.ReviewedBy.Should().Be(reviewer);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void File_Update_RejectsNonIncreasingRevision(long revision)
        {
            // Arrange
            var file = new WorkspaceFile(Fixture.Create<Guid>());
            var content = Fixture.Create<string>();

            WriteOutput("Arranged input", (nameof(file), file), (nameof(revision), revision), (nameof(content), content));

            // Act
            var result = Record.Exception(() => file.Update(file.FileName, file.Language, content, revision));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeOfType<BusinessRuleException>();
            file.SavedRevision.Should().Be(0);
        }

        #endregion
    }
}
