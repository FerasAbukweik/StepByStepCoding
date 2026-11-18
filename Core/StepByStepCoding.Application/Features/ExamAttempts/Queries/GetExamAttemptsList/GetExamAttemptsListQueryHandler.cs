using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.ExamAttempts.Queries.GetExamAttemptsList
{
    public sealed class GetExamAttemptsListQueryHandler : IRequestHandler<GetExamAttemptsListQuery, PaginatedDTO<ExamAttemptListDTO>>
    {
        private readonly IExamAttemptRepository repository;
        private readonly IAcademicAccess access;
        private readonly IUserDirectory users;

        public GetExamAttemptsListQueryHandler(IExamAttemptRepository repository, IAcademicAccess access, IUserDirectory users)
        {
            this.repository = repository;
            this.access = access;
            this.users = users;
        }

        public async Task<PaginatedDTO<ExamAttemptListDTO>> Handle(GetExamAttemptsListQuery request, CancellationToken cancellationToken)
        {
            var scope = access.CurrentScope();

            var result = await repository.GetPage(scope, request.ExamId, request.Page, request.RecordsPerPage, cancellationToken);

            var names = await users.GetNames(result.Items.Select(entity => entity.StudentId), cancellationToken);

            return new PaginatedDTO<ExamAttemptListDTO>
            {
                Items = result.Items.Select(entity => entity.ToDTO(names.GetValueOrDefault(entity.StudentId, "Student"))).ToArray(),
                TotalCount = result.TotalCount
            };
        }
    }
}
