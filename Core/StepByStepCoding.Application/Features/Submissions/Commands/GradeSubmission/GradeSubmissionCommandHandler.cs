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

namespace StepByStepCoding.Application.Features.Submissions.Commands.GradeSubmission
{
    public sealed class GradeSubmissionCommandHandler : IRequestHandler<GradeSubmissionCommand>
    {
        private readonly ISubmissionRepository repository;
        private readonly IExamQuestionRepository questions;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly TimeProvider clock;

        public GradeSubmissionCommandHandler(
            ISubmissionRepository repository,
            IExamQuestionRepository questions,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            TimeProvider clock)
        {
            this.repository = repository;
            this.questions = questions;
            this.unitOfWork = unitOfWork;
            this.access = access;
            this.clock = clock;
        }

        public async Task Handle(GradeSubmissionCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var scope = access.CurrentScope();

            var entity = await repository.GetById(request.Id,
                [
                    entity => entity.Attempt!.Exam!.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Submission was not found.");
            access.ManageSubmission(entity);
            CommandChecks.Version(entity, request.Version);
            var question = await questions.GetById(entity.QuestionId,
                [
                    entity => entity.Exam!.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Question was not found.");
            entity.Grade(request.Score, question.MaxScore, request.Feedback, scope.UserId, clock.GetUtcNow());

            try
            {
                await repository.Update(entity);
                await unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                await unitOfWork.Rollback(CancellationToken.None);
                throw;
            }

        }
    }
}
