using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Features.Common;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Domain.Common;

namespace StepByStepCoding.Application.Features.Submissions.Commands.SubmitCode
{
    public sealed class SubmitCodeCommandHandler : IRequestHandler<SubmitCodeCommand, Guid>
    {
        private readonly ISubmissionRepository repository;
        private readonly IExamAttemptRepository attempts;
        private readonly IExamQuestionRepository questions;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly TimeProvider clock;

        public SubmitCodeCommandHandler(
            ISubmissionRepository repository,
            IExamAttemptRepository attempts,
            IExamQuestionRepository questions,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            TimeProvider clock)
        {
            this.repository = repository;
            this.attempts = attempts;
            this.questions = questions;
            this.unitOfWork = unitOfWork;
            this.access = access;
            this.clock = clock;
        }

        public async Task<Guid> Handle(SubmitCodeCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var scope = access.CurrentScope();

            var studentId = scope.UserId;
            var attempt = await attempts.GetById(request.AttemptId,
                [
                    entity => entity.Exam!.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Attempt was not found.");
            access.ReadAttempt(attempt);

            if (attempt.StudentId != studentId)
                throw new ForbiddenException("You cannot submit for another student.");

            var exam = attempt.Exam!;
            access.ReadExam(exam);
            var question = await questions.GetById(request.QuestionId,
                [
                    entity => entity.Exam!.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Question was not found.");
            var now = clock.GetUtcNow();
            AcademicRules.Require(question.ExamId == exam.Id, "The question does not belong to this attempt's exam.");
            AcademicRules.Require(exam.Status == ExamStatus.Published && now < exam.Deadline, "The exam is closed.");
            attempt.RegisterSubmission(now);

            if (await repository.Any(existing => existing.AttemptId == attempt.Id && existing.QuestionId == question.Id, cancellationToken))
                throw new ConflictException("A submission already exists for this question and attempt.");

            var entity = new Submission(attempt.Id, question.Id, request.SubmittedCode, request.Language, now);

            try
            {
                await attempts.Update(attempt);
                await repository.Add(entity);
                await unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                await unitOfWork.Rollback(CancellationToken.None);
                throw;
            }

            return entity.Id;
        }
    }
}
