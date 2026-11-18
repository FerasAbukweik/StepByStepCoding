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

namespace StepByStepCoding.Application.Features.Lectures.Commands.UpdateLecture
{
    public sealed class UpdateLectureCommandHandler : IRequestHandler<UpdateLectureCommand>
    {
        private readonly ILectureFileRepository files;
        private readonly ILectureRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;

        public UpdateLectureCommandHandler(
            ILectureRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            ILectureFileRepository files)
        {
            this.files = files;
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
        }

        public async Task Handle(UpdateLectureCommand request, CancellationToken cancellationToken)
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

            entity.Update(request.Title, request.ScheduledStart);

            try
            {
                if (request.FileName is not null)
                {
                    if (entity.File is null)
                        await files.Add(new LectureFile(entity.Id, request.FileName, request.Language, request.InitialCode, 0));
                    else
                    {
                        if (request.FileId != entity.File.Id)
                            throw new CustomValidationException("The file does not belong to this lecture.");

                        CommandChecks.Version(entity.File, request.FileVersion ?? Guid.Empty);
                        entity.File.Update(request.FileName, request.Language, request.InitialCode, checked(entity.File.SavedRevision + 1));
                        await files.Update(entity.File);
                    }
                }

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
