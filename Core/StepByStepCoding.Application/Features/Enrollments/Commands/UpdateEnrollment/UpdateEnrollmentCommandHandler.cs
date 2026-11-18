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

namespace StepByStepCoding.Application.Features.Enrollments.Commands.UpdateEnrollment
{
    public sealed class UpdateEnrollmentCommandHandler : IRequestHandler<UpdateEnrollmentCommand>
    {
        private readonly IEnrollmentRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;

        public UpdateEnrollmentCommandHandler(
            IEnrollmentRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
        }

        public async Task Handle(UpdateEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var entity = await repository.GetById(request.Id, [entity => entity.Section], cancellationToken)
                ?? throw new NotFoundException("Enrollment was not found.");
            access.ManageSection(entity.Section!);
            CommandChecks.Version(entity, request.Version);

            entity.Update(request.Active);

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
