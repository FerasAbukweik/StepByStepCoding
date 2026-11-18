using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Submissions.Queries.GetSubmissionsList
{
    public sealed class GetSubmissionsListQueryHandler : IRequestHandler<GetSubmissionsListQuery, PaginatedDTO<SubmissionListDTO>>
    {
        private readonly ISubmissionRepository repository;
        private readonly IAcademicAccess access;

        public GetSubmissionsListQueryHandler(ISubmissionRepository repository, IAcademicAccess access)
        {
            this.repository = repository;
            this.access = access;
        }

        public async Task<PaginatedDTO<SubmissionListDTO>> Handle(GetSubmissionsListQuery request, CancellationToken cancellationToken)
        {
            var scope = access.CurrentScope();

            var result = await repository.GetPage(scope, request.AttemptId, request.Page, request.RecordsPerPage, request.QuestionId, cancellationToken);

            return new PaginatedDTO<SubmissionListDTO>
            {
                Items = result.Items.Select(entity => entity.ToDTO()).ToArray(),
                TotalCount = result.TotalCount
            };
        }
    }
}
