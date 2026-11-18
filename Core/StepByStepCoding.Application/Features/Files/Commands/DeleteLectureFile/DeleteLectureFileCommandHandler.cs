using StepByStepCoding.Application.Contracts.Realtime;
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

namespace StepByStepCoding.Application.Features.Files.Commands.DeleteLectureFile
{
    public sealed class DeleteLectureFileCommandHandler : IRequestHandler<DeleteLectureFileCommand>
    {
        private readonly ILectureFileRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ISignalRService signalR;
        private readonly IAcademicAccess access;
        private readonly ILectureRepository lectures;

        public DeleteLectureFileCommandHandler(
            ILectureFileRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            ILectureRepository lectures,
            ISignalRService signalR)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.signalR = signalR;
            this.access = access;
            this.lectures = lectures;
        }

        public async Task Handle(DeleteLectureFileCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await repository.GetWithEnrollment(request.Id, userId, cancellationToken)
                ?? throw new NotFoundException("Lecture file was not found.");
            access.ManageFile(entity);
            var lecture = entity.Lecture!;
            if (lecture.Status == LectureStatus.Live)
                throw new ConflictException("The code file cannot be deleted during a live lecture.");

            lecture.RegisterFileChange();
            CommandChecks.Version(entity, request.Version);

            if (await repository.HasDependents(entity.Id, cancellationToken))
                throw new ConflictException("Remove related records before deleting this record.");

            try
            {
                await lectures.Update(lecture);
                await repository.Delete(entity);
                await unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                await unitOfWork.Rollback(CancellationToken.None);
                throw;
            }

            await signalR.FileDeleted(entity.LectureId, entity.Id);

        }
    }
}
