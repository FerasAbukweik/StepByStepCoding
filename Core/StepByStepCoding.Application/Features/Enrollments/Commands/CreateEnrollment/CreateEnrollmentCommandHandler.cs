using StepByStepCoding.Application.Enums;
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

namespace StepByStepCoding.Application.Features.Enrollments.Commands.CreateEnrollment
{
    public sealed class CreateEnrollmentCommandHandler : IRequestHandler<CreateEnrollmentCommand, Guid>
    {
        private readonly IEnrollmentRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly ISectionRepository sectionRepository;
        private readonly IUserDirectory users;

        public CreateEnrollmentCommandHandler(
            IEnrollmentRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            IUserDirectory users,
            ISectionRepository sectionRepository)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
            this.sectionRepository = sectionRepository;
            this.users = users;
        }

        public async Task<Guid> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var sectionForAccess = await sectionRepository.GetById(request.SectionId,
                [
                    entity => entity.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Section was not found.");
            access.ManageSection(sectionForAccess);

            if (!await users.IsInRole(request.StudentId, nameof(UserRole.Student), cancellationToken))
                throw new CustomValidationException("The enrolled user must be a student.");

            if (await repository.Any(existing => existing.SectionId == request.SectionId && existing.StudentId == request.StudentId, cancellationToken))
                throw new ConflictException("A record with these details already exists.");

            var entity = new Enrollment(request.SectionId, request.StudentId);

            try
            {
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
