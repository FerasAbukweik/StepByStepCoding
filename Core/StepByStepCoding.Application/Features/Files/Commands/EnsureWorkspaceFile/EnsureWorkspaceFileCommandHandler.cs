using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Files.Commands.EnsureWorkspaceFile
{
    public sealed class EnsureWorkspaceFileCommandHandler : IRequestHandler<EnsureWorkspaceFileCommand, Guid>
    {
        private readonly IAcademicAccess access;
        private readonly IWorkspaceFileRepository repository;
        private readonly IUnitOfWork unitOfWork;

        public EnsureWorkspaceFileCommandHandler(IAcademicAccess access, IWorkspaceFileRepository repository, IUnitOfWork unitOfWork)
        {
            this.access = access;
            this.repository = repository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(EnsureWorkspaceFileCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;
            var existing = await repository.GetByUser(userId, cancellationToken);

            if (existing is not null)
                return existing.Id;

            var file = new WorkspaceFile(userId);

            try
            {
                await repository.Add(file);
                await unitOfWork.Commit(cancellationToken);
            }
            catch (ConflictException)
            {
                await unitOfWork.Rollback(CancellationToken.None);
                // Concurrent Me requests can create the same user's file; the unique index decides the winner.
                existing = await repository.GetByUser(userId, cancellationToken);

                if (existing is not null)
                    return existing.Id;

                throw;
            }
            catch
            {
                await unitOfWork.Rollback(CancellationToken.None);
                throw;
            }

            return file.Id;
        }
    }
}
