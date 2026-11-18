using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Features.Common;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Domain.Common;

namespace StepByStepCoding.Application.Features.Courses.Commands.DeleteCourse
{
    public sealed class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand>
    {
        private readonly ICourseRepository repository;
        private readonly IUnitOfWork unitOfWork;

        public DeleteCourseCommandHandler(
            ICourseRepository repository,
            IUnitOfWork unitOfWork)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
        {
            var entity = await repository.GetById(request.Id) ?? throw new NotFoundException("Course was not found.");
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
