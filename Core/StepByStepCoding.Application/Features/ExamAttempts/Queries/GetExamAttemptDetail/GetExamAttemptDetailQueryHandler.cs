using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.ExamAttempts.Queries.GetExamAttemptDetail
{
    public sealed class GetExamAttemptDetailQueryHandler : IRequestHandler<GetExamAttemptDetailQuery, ExamAttemptDetailDTO>
    {
        private readonly IAcademicAccess access;
        private readonly IExamAttemptRepository examAttemptRepository;

        public GetExamAttemptDetailQueryHandler(IAcademicAccess access,
            IExamAttemptRepository examAttemptRepository)
        {
            this.access = access;
            this.examAttemptRepository = examAttemptRepository;
        }

        public async Task<ExamAttemptDetailDTO> Handle(GetExamAttemptDetailQuery request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await examAttemptRepository.GetById(request.Id,
                [
                    entity => entity.Exam!.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Attempt was not found.");
            access.ReadAttempt(entity);

            return entity.ToDTO();
        }
    }
}
