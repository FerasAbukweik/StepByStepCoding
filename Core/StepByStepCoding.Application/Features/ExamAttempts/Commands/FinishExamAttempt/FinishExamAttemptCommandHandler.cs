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

namespace StepByStepCoding.Application.Features.ExamAttempts.Commands.FinishExamAttempt
{
    public sealed class FinishExamAttemptCommandHandler : IRequestHandler<FinishExamAttemptCommand>
    {
        private readonly IExamAttemptRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly TimeProvider clock;

        public FinishExamAttemptCommandHandler(
            IExamAttemptRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            TimeProvider clock)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
            this.clock = clock;
        }

        public async Task Handle(FinishExamAttemptCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var scope = access.CurrentScope();

            var studentId = scope.UserId;
            var entity = await repository.GetById(request.Id,
                [
                    entity => entity.Exam!.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Attempt was not found.");
            access.ReadAttempt(entity);

            if (entity.StudentId != studentId)
                throw new ForbiddenException("You cannot finish another student's attempt.");

            CommandChecks.Version(entity, request.Version);
            entity.Finish(clock.GetUtcNow());

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
