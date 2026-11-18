using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Features.Files.Queries.GetFileDetail;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Files.Queries.GetLectureFileByLecture
{
    public sealed class GetLectureFileByLectureQueryHandler : IRequestHandler<GetLectureFileByLectureQuery, FileDetailDTO>
    {
        private readonly IAcademicAccess access;
        private readonly ILectureRepository lectureRepository;

        public GetLectureFileByLectureQueryHandler(IAcademicAccess access,
            ILectureRepository lectureRepository)
        {
            this.access = access;
            this.lectureRepository = lectureRepository;
        }

        public async Task<FileDetailDTO> Handle(GetLectureFileByLectureQuery request, CancellationToken cancellationToken)
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

            return file.ToDTO();
        }
    }
}
