using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Submissions.Queries.GetSubmissionDetail
{
    public sealed class GetSubmissionDetailQueryHandler : IRequestHandler<GetSubmissionDetailQuery, SubmissionDetailDTO>
    {
        private readonly IAcademicAccess access;
        private readonly ISubmissionRepository submissionRepository;

        public GetSubmissionDetailQueryHandler(IAcademicAccess access,
            ISubmissionRepository submissionRepository)
        {
            this.access = access;
            this.submissionRepository = submissionRepository;
        }

        public async Task<SubmissionDetailDTO> Handle(GetSubmissionDetailQuery request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await submissionRepository.GetById(request.Id,
                [
                    entity => entity.Attempt!.Exam!.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Submission was not found.");
            access.ReadSubmission(entity);

            return entity.ToDTO();
        }
    }
}
