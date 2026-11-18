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

namespace StepByStepCoding.Application.Features.Lectures.Commands.DeleteLecture
{
    public sealed class DeleteLectureCommandHandler : IRequestHandler<DeleteLectureCommand>
    {
        private readonly ILectureFileRepository files;
        private readonly ISignalRService signalR;
        private readonly ILectureRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;

        public DeleteLectureCommandHandler(
            ILectureRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            ILectureFileRepository files,
            ISignalRService signalR)
        {
            this.files = files;
            this.signalR = signalR;
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
        }

        public async Task Handle(DeleteLectureCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await repository.GetById(request.Id,
                [
                    entity => entity.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active),
                    entity => entity.File
                ], cancellationToken)
                ?? throw new NotFoundException("Lecture was not found.");
            access.ManageLecture(entity);
            CommandChecks.Version(entity, request.Version);
            AcademicRules.Require(entity.Status == LectureStatus.Scheduled, "Only scheduled lectures can be deleted.");

            try
            {
                if (entity.File is not null)
                    await files.Delete(entity.File);

                await repository.Delete(entity);
                await unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                await unitOfWork.Rollback(CancellationToken.None);
                throw;
            }

            if (entity.File is not null)
                await signalR.FileDeleted(entity.Id, entity.File.Id);
        }
    }
}
