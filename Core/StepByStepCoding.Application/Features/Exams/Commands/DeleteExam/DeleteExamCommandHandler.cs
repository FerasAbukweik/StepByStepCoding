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

namespace StepByStepCoding.Application.Features.Exams.Commands.DeleteExam
{
    public sealed class DeleteExamCommandHandler : IRequestHandler<DeleteExamCommand>
    {
        private readonly IExamRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;

        public DeleteExamCommandHandler(
            IExamRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
        }

        public async Task Handle(DeleteExamCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await repository.GetById(request.Id,
                [
                    entity => entity.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Exam was not found.");
            access.ManageExam(entity);
            CommandChecks.Version(entity, request.Version);
            entity.EnsureDraft();

            if (await repository.HasDependents(entity.Id, cancellationToken))
                throw new ConflictException("Remove related records before deleting this record.");

            try
            {
                await repository.Delete(entity);
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
