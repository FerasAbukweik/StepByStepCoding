using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentsList
{
    public sealed class GetEnrollmentsListQueryHandler : IRequestHandler<GetEnrollmentsListQuery, PaginatedDTO<EnrollmentListDTO>>
    {
        private readonly IEnrollmentRepository repository;
        private readonly IAcademicAccess access;
        private readonly IUserDirectory users;

        public GetEnrollmentsListQueryHandler(IEnrollmentRepository repository, IAcademicAccess access, IUserDirectory users)
        {
            this.repository = repository;
            this.access = access;
            this.users = users;
        }

        public async Task<PaginatedDTO<EnrollmentListDTO>> Handle(GetEnrollmentsListQuery request, CancellationToken cancellationToken)
        {
            var scope = access.CurrentScope();

            var result = await repository.GetPage(scope, request.SectionId, request.Page, request.RecordsPerPage, cancellationToken);

            var names = await users.GetNames(result.Items.Select(entity => entity.StudentId), cancellationToken);

            return new PaginatedDTO<EnrollmentListDTO>
            {
                Items = result.Items.Select(entity => entity.ToDTO(names.GetValueOrDefault(entity.StudentId, "Student"))).ToArray(),
                TotalCount = result.TotalCount
            };
        }
    }
}
