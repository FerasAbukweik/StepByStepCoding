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

namespace StepByStepCoding.Application.Features.Sections.Commands.CreateSection
{
    public sealed class CreateSectionCommandHandler : IRequestHandler<CreateSectionCommand, Guid>
    {
        private readonly ISectionRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ICourseRepository courses;
        private readonly IUserDirectory users;

        public CreateSectionCommandHandler(
            ISectionRepository repository,
            IUnitOfWork unitOfWork,
            ICourseRepository courses,
            IUserDirectory users)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.courses = courses;
            this.users = users;
        }

        public async Task<Guid> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
        {
            _ = await courses.GetById(request.CourseId) ?? throw new NotFoundException("Course was not found.");

            if (!await users.IsInRole(request.InstructorId, nameof(UserRole.Instructor), cancellationToken))
                throw new CustomValidationException("The assigned user must be an instructor.");

            var entity = new Section(request.CourseId, request.InstructorId, request.Name);

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
