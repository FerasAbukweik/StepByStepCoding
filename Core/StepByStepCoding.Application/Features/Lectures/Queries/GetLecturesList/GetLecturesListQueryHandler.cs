using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Lectures.Queries.GetLecturesList
{
    public sealed class GetLecturesListQueryHandler : IRequestHandler<GetLecturesListQuery, PaginatedDTO<LectureListDTO>>
    {
        private readonly ILectureRepository repository;
        private readonly IAcademicAccess access;

        public GetLecturesListQueryHandler(ILectureRepository repository, IAcademicAccess access)
        {
            this.repository = repository;
            this.access = access;
        }

        public async Task<PaginatedDTO<LectureListDTO>> Handle(GetLecturesListQuery request, CancellationToken cancellationToken)
        {
            var scope = access.CurrentScope();

            var result = await repository.GetPage(scope, request.SectionId, request.Page, request.RecordsPerPage, cancellationToken);

            return new PaginatedDTO<LectureListDTO>
            {
                Items = result.Items.Select(entity => entity.ToDTO()).ToArray(),
                TotalCount = result.TotalCount
            };
        }
    }
}
