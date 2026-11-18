using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Exams.Queries.GetExamsList
{
    public sealed class GetExamsListQueryHandler : IRequestHandler<GetExamsListQuery, PaginatedDTO<ExamListDTO>>
    {
        private readonly IExamRepository repository;
        private readonly IAcademicAccess access;

        public GetExamsListQueryHandler(IExamRepository repository, IAcademicAccess access)
        {
            this.repository = repository;
            this.access = access;
        }

        public async Task<PaginatedDTO<ExamListDTO>> Handle(GetExamsListQuery request, CancellationToken cancellationToken)
        {
            var scope = access.CurrentScope();

            var result = await repository.GetPage(scope, request.SectionId, request.Page, request.RecordsPerPage, cancellationToken);

            return new PaginatedDTO<ExamListDTO>
            {
                Items = result.Items.Select(entity => entity.ToDTO()).ToArray(),
                TotalCount = result.TotalCount
            };
        }
    }
}
