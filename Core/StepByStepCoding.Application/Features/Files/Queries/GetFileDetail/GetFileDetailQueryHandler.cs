using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Files.Queries.GetFileDetail
{
    public sealed class GetFileDetailQueryHandler : IRequestHandler<GetFileDetailQuery, FileDetailDTO>
    {
        private readonly IAcademicAccess access;
        private readonly IFileRepository lectureFileRepository;

        public GetFileDetailQueryHandler(IAcademicAccess access,
            IFileRepository lectureFileRepository)
        {
            this.access = access;
            this.lectureFileRepository = lectureFileRepository;
        }

        public async Task<FileDetailDTO> Handle(GetFileDetailQuery request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await lectureFileRepository.GetWithEnrollment(request.Id, userId, cancellationToken)
                ?? throw new NotFoundException("File was not found.");
            access.ReadFile(entity);

            return entity.ToDTO();
        }
    }
}
