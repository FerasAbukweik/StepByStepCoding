using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Courses.Queries.GetCoursesList
{
    public sealed class GetCoursesListQueryHandler : IRequestHandler<GetCoursesListQuery, PaginatedDTO<CourseListDTO>>
    {
        private readonly ICourseRepository repository;
        private readonly IAcademicAccess access;

        public GetCoursesListQueryHandler(ICourseRepository repository, IAcademicAccess access)
        {
            this.repository = repository;
            this.access = access;
        }

        public async Task<PaginatedDTO<CourseListDTO>> Handle(GetCoursesListQuery request, CancellationToken cancellationToken)
        {
            var scope = access.CurrentScope();

            var result = await repository.GetPage(scope, null, request.Page, request.RecordsPerPage, cancellationToken);

            return new PaginatedDTO<CourseListDTO>
            {
                Items = result.Items.Select(entity => entity.ToDTO()).ToArray(),
                TotalCount = result.TotalCount
            };
        }
    }
}
