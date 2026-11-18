using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Courses.Queries.GetCourseDetail
{
    public sealed class GetCourseDetailQueryHandler : IRequestHandler<GetCourseDetailQuery, CourseDetailDTO>
    {
        private readonly ICourseRepository repository;
        private readonly IAcademicAccess access;

        public GetCourseDetailQueryHandler(ICourseRepository repository, IAcademicAccess access)
        {
            this.repository = repository;
            this.access = access;
        }

        public async Task<CourseDetailDTO> Handle(GetCourseDetailQuery request, CancellationToken cancellationToken)
        {
            access.CurrentScope();
            var entity = await repository.GetById(request.Id) ?? throw new NotFoundException("Course was not found.");

            return entity.ToDTO();
        }
    }
}
