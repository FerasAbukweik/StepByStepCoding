using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Realtime;
using StepByStepCoding.Application.Services;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Files.Commands.SaveChanges
{
    public sealed class SaveChangesCommandHandler : IRequestHandler<SaveChangesCommand, CodeChangesDTO>
    {
        private readonly IAcademicAccess access;
        private readonly IFileRepository repository;
        private readonly IUnitOfWork unitOfWork;

        public SaveChangesCommandHandler(IAcademicAccess access, IFileRepository repository, IUnitOfWork unitOfWork)
        {
            this.access = access;
            this.repository = repository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<CodeChangesDTO> Handle(SaveChangesCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var file = await repository.GetWithEnrollment(request.FileId, userId, cancellationToken)
                ?? throw new NotFoundException("File was not found.");

            access.ManageFile(file);

            if (file is LectureFile lectureFile && lectureFile.Lecture!.Status != LectureStatus.Live)
                throw new ConflictException("Code changes require a live lecture.");

            if (file.SavedRevision != request.BaseRevision)
                throw new ConflictException("The code revision changed. Request the file again.");

            var content = CodeEditApplicator.Apply(file.SavedContent, request.ChangeSets);
            file.Update(request.FileName ?? file.FileName, request.Language ?? file.Language, content, checked(file.SavedRevision + 1));

            try
            {
                await repository.Update(file);
                await unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                await unitOfWork.Rollback(CancellationToken.None);
                throw;
            }

            return new CodeChangesDTO
            {
                FileId = file.Id,
                FileName = file.FileName,
                Language = file.Language,
                LectureId = (file as LectureFile)?.LectureId,
                BatchId = request.BatchId,
                BaseRevision = request.BaseRevision,
                Revision = file.SavedRevision,
                ChangeSets = request.ChangeSets
            };
        }
    }
}
