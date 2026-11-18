using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Lectures.Queries.GetLectureDetail
{
    public sealed class GetLectureDetailQueryHandler : IRequestHandler<GetLectureDetailQuery, LectureDetailDTO>
    {
        private readonly IAcademicAccess access;
        private readonly ILectureRepository lectureRepository;

        public GetLectureDetailQueryHandler(IAcademicAccess access,
            ILectureRepository lectureRepository)
        {
            this.access = access;
            this.lectureRepository = lectureRepository;
        }

        public async Task<LectureDetailDTO> Handle(GetLectureDetailQuery request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await lectureRepository.GetById(request.Id,
                [
                    entity => entity.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active),
                    entity => entity.File
                ], cancellationToken)
                ?? throw new NotFoundException("Lecture was not found.");
            access.ReadLecture(entity);

            return entity.ToDTO();
        }
    }
}
