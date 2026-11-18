using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.ExamQuestions.Queries.GetExamQuestionsList
{
    public sealed class GetExamQuestionsListQueryHandler : IRequestHandler<GetExamQuestionsListQuery, PaginatedDTO<ExamQuestionListDTO>>
    {
        private readonly IExamQuestionRepository repository;
        private readonly IAcademicAccess access;

        public GetExamQuestionsListQueryHandler(IExamQuestionRepository repository, IAcademicAccess access)
        {
            this.repository = repository;
            this.access = access;
        }

        public async Task<PaginatedDTO<ExamQuestionListDTO>> Handle(GetExamQuestionsListQuery request, CancellationToken cancellationToken)
        {
            var scope = access.CurrentScope();

            var result = await repository.GetPage(scope, request.ExamId, request.Page, request.RecordsPerPage, cancellationToken);

            return new PaginatedDTO<ExamQuestionListDTO>
            {
                Items = result.Items.Select(entity => entity.ToDTO()).ToArray(),
                TotalCount = result.TotalCount
            };
        }
    }
}
