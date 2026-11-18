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

namespace StepByStepCoding.Application.Features.Lectures.Commands.CreateLecture
{
    public sealed class CreateLectureCommandHandler : IRequestHandler<CreateLectureCommand, Guid>
    {
        private readonly ILectureFileRepository files;
        private readonly ILectureRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly ISectionRepository sectionRepository;

        public CreateLectureCommandHandler(
            ILectureRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            ISectionRepository sectionRepository,
            ILectureFileRepository files)
        {
            this.files = files;
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
            this.sectionRepository = sectionRepository;
        }

        public async Task<Guid> Handle(CreateLectureCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var sectionForAccess = await sectionRepository.GetById(request.SectionId,
                [
                    entity => entity.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Section was not found.");
            access.ManageSection(sectionForAccess);

            var entity = new Lecture(request.SectionId, request.Title, request.ScheduledStart);

            try
            {
                await repository.Add(entity);

                // One save keeps creation atomic when the UI supplies the lecture file.
                if (request.FileName is not null)
                    await files.Add(new LectureFile(entity.Id, request.FileName, request.Language, request.InitialCode, 0));

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
