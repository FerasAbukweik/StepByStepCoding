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

namespace StepByStepCoding.Application.Features.Files.Commands.UpdateLectureFile
{
    public sealed class UpdateLectureFileCommandHandler : IRequestHandler<UpdateLectureFileCommand>
    {
        private readonly ILectureFileRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly ILectureRepository lectures;

        public UpdateLectureFileCommandHandler(
            ILectureFileRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            ILectureRepository lectures)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
            this.lectures = lectures;
        }

        public async Task Handle(UpdateLectureFileCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await repository.GetWithEnrollment(request.Id, userId, cancellationToken)
                ?? throw new NotFoundException("Lecture file was not found.");
            access.ManageFile(entity);
            var lecture = entity.Lecture!;
            if (lecture.Status == LectureStatus.Live)
                throw new ConflictException("Use SaveChanges to edit code during a live lecture.");

            lecture.RegisterFileChange();
            CommandChecks.Version(entity, request.Version);
            entity.Update(request.FileName, request.Language, request.SavedContent, request.SavedRevision);

            try
            {
                await lectures.Update(lecture);
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
