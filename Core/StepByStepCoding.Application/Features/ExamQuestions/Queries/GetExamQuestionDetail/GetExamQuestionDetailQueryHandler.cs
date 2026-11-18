using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.ExamQuestions.Queries.GetExamQuestionDetail
{
    public sealed class GetExamQuestionDetailQueryHandler : IRequestHandler<GetExamQuestionDetailQuery, ExamQuestionDetailDTO>
    {
        private readonly IAcademicAccess access;
        private readonly IExamQuestionRepository examQuestionRepository;

        public GetExamQuestionDetailQueryHandler(IAcademicAccess access,
            IExamQuestionRepository examQuestionRepository)
        {
            this.access = access;
            this.examQuestionRepository = examQuestionRepository;
        }

        public async Task<ExamQuestionDetailDTO> Handle(GetExamQuestionDetailQuery request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await examQuestionRepository.GetById(request.Id,
                [
                    entity => entity.Exam!.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Question was not found.");
            access.ReadQuestion(entity);

            return entity.ToDTO();
        }
    }
}
