using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Exams.Queries.GetExamDetail
{
    public sealed class GetExamDetailQueryHandler : IRequestHandler<GetExamDetailQuery, ExamDetailDTO>
    {
        private readonly IAcademicAccess access;
        private readonly IExamRepository examRepository;

        public GetExamDetailQueryHandler(IAcademicAccess access,
            IExamRepository examRepository)
        {
            this.access = access;
            this.examRepository = examRepository;
        }

        public async Task<ExamDetailDTO> Handle(GetExamDetailQuery request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await examRepository.GetById(request.Id,
                [
                    entity => entity.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Exam was not found.");
            access.ReadExam(entity);

            return entity.ToDTO();
        }
    }
}
