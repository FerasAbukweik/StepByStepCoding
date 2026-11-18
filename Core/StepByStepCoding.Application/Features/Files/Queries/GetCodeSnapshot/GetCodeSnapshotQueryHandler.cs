using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Models.Realtime;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Files.Queries.GetCodeSnapshot
{
    public sealed class GetCodeSnapshotQueryHandler : IRequestHandler<GetCodeSnapshotQuery, CodeSnapshotDTO>
    {
        private readonly IAcademicAccess access;
        private readonly ILectureRepository lectureRepository;

        public GetCodeSnapshotQueryHandler(IAcademicAccess access,
            ILectureRepository lectureRepository)
        {
            this.access = access;
            this.lectureRepository = lectureRepository;
        }

        public async Task<CodeSnapshotDTO> Handle(GetCodeSnapshotQuery request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var lecture = await lectureRepository.GetById(request.LectureId,
                [
                    entity => entity.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active),
                    entity => entity.File
                ], cancellationToken)
                ?? throw new NotFoundException("Lecture was not found.");
            access.ReadLecture(lecture);
            var file = lecture.File ?? throw new NotFoundException("Lecture file was not found.");
            access.ReadFile(file);

            return new CodeSnapshotDTO
            {
                LectureId = file.LectureId,
                FileId = file.Id,
                FileName = file.FileName,
                FileType = file.Language,
                Code = file.SavedContent,
                Revision = file.SavedRevision
            };
        }
    }
}
