using StepByStepCoding.Application.Enums;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Features.Common;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Domain.Common;

namespace StepByStepCoding.Application.Features.Sections.Commands.UpdateSection
{
    public sealed class UpdateSectionCommandHandler : IRequestHandler<UpdateSectionCommand>
    {
        private readonly ISectionRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserDirectory users;

        public UpdateSectionCommandHandler(
            ISectionRepository repository,
            IUnitOfWork unitOfWork,
            IUserDirectory users)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.users = users;
        }

        public async Task Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
        {
            var entity = await repository.GetById(request.Id) ?? throw new NotFoundException("Section was not found.");
            CommandChecks.Version(entity, request.Version);

            if (!await users.IsInRole(request.InstructorId, nameof(UserRole.Instructor), cancellationToken))
                throw new CustomValidationException("The assigned user must be an instructor.");

            entity.Update(request.InstructorId, request.Name);

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
