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

namespace StepByStepCoding.Application.Features.Files.Commands.CreateLectureFile
{
    public sealed class CreateLectureFileCommandHandler : IRequestHandler<CreateLectureFileCommand, Guid>
    {
        private readonly ILectureFileRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly ILectureRepository lectures;

        public CreateLectureFileCommandHandler(
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

        public async Task<Guid> Handle(CreateLectureFileCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var lecture = await lectures.GetById(request.LectureId,
                [
                    entity => entity.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active),
                    entity => entity.File
                ], cancellationToken)
                ?? throw new NotFoundException("Lecture was not found.");
            access.ManageLecture(lecture);
            lecture.RegisterFileChange();

            var entity = new LectureFile(request.LectureId, request.FileName, request.Language, request.SavedContent, request.SavedRevision);

            try
            {
                await lectures.Update(lecture);
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
