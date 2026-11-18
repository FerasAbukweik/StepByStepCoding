using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Sections.Queries.GetSectionsList
{
    public sealed class GetSectionsListQueryHandler : IRequestHandler<GetSectionsListQuery, PaginatedDTO<SectionListDTO>>
    {
        private readonly ISectionRepository repository;
        private readonly IAcademicAccess access;
        private readonly IUserDirectory users;

        public GetSectionsListQueryHandler(ISectionRepository repository, IAcademicAccess access, IUserDirectory users)
        {
            this.repository = repository;
            this.access = access;
            this.users = users;
        }

        public async Task<PaginatedDTO<SectionListDTO>> Handle(GetSectionsListQuery request, CancellationToken cancellationToken)
        {
            var scope = access.CurrentScope();

            var result = await repository.GetPage(scope, request.CourseId, request.Page, request.RecordsPerPage, cancellationToken);
            var names = await users.GetNames(result.Items.Select(entity => entity.InstructorId), cancellationToken);

            return new PaginatedDTO<SectionListDTO>
            {
                Items = result.Items.Select(entity => entity.ToDTO(names.GetValueOrDefault(entity.InstructorId, "Instructor"))).ToArray(),
                TotalCount = result.TotalCount
            };
        }
    }
}
