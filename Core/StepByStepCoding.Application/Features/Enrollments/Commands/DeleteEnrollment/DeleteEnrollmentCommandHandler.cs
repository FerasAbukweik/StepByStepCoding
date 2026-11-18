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

namespace StepByStepCoding.Application.Features.Enrollments.Commands.DeleteEnrollment
{
    public sealed class DeleteEnrollmentCommandHandler : IRequestHandler<DeleteEnrollmentCommand>
    {
        private readonly IEnrollmentRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;

        public DeleteEnrollmentCommandHandler(
            IEnrollmentRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
        }

        public async Task Handle(DeleteEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var entity = await repository.GetById(request.Id, [entity => entity.Section], cancellationToken)
                ?? throw new NotFoundException("Enrollment was not found.");
            access.ManageSection(entity.Section!);
            CommandChecks.Version(entity, request.Version);

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
