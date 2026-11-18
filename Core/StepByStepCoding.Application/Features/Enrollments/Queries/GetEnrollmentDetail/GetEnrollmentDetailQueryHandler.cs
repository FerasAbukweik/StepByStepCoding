using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentDetail
{
    public sealed class GetEnrollmentDetailQueryHandler : IRequestHandler<GetEnrollmentDetailQuery, EnrollmentDetailDTO>
    {
        private readonly IAcademicAccess access;
        private readonly IEnrollmentRepository enrollmentRepository;

        public GetEnrollmentDetailQueryHandler(IAcademicAccess access,
            IEnrollmentRepository enrollmentRepository)
        {
            this.access = access;
            this.enrollmentRepository = enrollmentRepository;
        }

        public async Task<EnrollmentDetailDTO> Handle(GetEnrollmentDetailQuery request, CancellationToken cancellationToken)
        {
            var entity = await enrollmentRepository.GetById(request.Id, [entity => entity.Section], cancellationToken)
                ?? throw new NotFoundException("Enrollment was not found.");
            access.ReadEnrollment(entity);

            return entity.ToDTO();
        }
    }
}
