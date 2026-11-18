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

namespace StepByStepCoding.Application.Features.Exams.Commands.PublishExam
{
    public sealed class PublishExamCommandHandler : IRequestHandler<PublishExamCommand>
    {
        private readonly IExamRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly TimeProvider clock;
        private readonly IExamQuestionRepository questions;

        public PublishExamCommandHandler(
            IExamRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            TimeProvider clock,
            IExamQuestionRepository questions)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
            this.clock = clock;
            this.questions = questions;
        }

        public async Task Handle(PublishExamCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var scope = access.CurrentScope();

            var entity = await repository.GetById(request.Id,
                [
                    entity => entity.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Exam was not found.");
            access.ManageExam(entity);
            CommandChecks.Version(entity, request.Version);
            AcademicRules.Require(entity.Deadline > clock.GetUtcNow(), "An expired exam cannot be published.");
            var count = await questions.Count(scope, entity.Id, cancellationToken);
            entity.Publish(count);

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
