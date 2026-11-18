using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Files.Queries.GetLectureFilesList
{
    public sealed class GetLectureFilesListQueryHandler : IRequestHandler<GetLectureFilesListQuery, PaginatedDTO<LectureFileListDTO>>
    {
        private readonly ILectureFileRepository repository;
        private readonly IAcademicAccess access;

        public GetLectureFilesListQueryHandler(ILectureFileRepository repository, IAcademicAccess access)
        {
            this.repository = repository;
            this.access = access;
        }

        public async Task<PaginatedDTO<LectureFileListDTO>> Handle(GetLectureFilesListQuery request, CancellationToken cancellationToken)
        {
            var scope = access.CurrentScope();

            var result = await repository.GetPage(scope, request.LectureId, request.Page, request.RecordsPerPage, cancellationToken);

            return new PaginatedDTO<LectureFileListDTO>
            {
                Items = result.Items.Select(entity => entity.ToDTO()).ToArray(),
                TotalCount = result.TotalCount
            };
        }
    }
}
