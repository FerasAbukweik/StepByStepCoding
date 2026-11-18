using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Sections.Queries.GetSectionDetail
{
    public sealed class GetSectionDetailQueryHandler : IRequestHandler<GetSectionDetailQuery, SectionDetailDTO>
    {
        private readonly IAcademicAccess access;
        private readonly IUserDirectory users;
        private readonly ISectionRepository sectionRepository;

        public GetSectionDetailQueryHandler(IAcademicAccess access,
            ISectionRepository sectionRepository, IUserDirectory users)
        {
            this.access = access;
            this.users = users;
            this.sectionRepository = sectionRepository;
        }

        public async Task<SectionDetailDTO> Handle(GetSectionDetailQuery request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await sectionRepository.GetById(request.Id,
                [
                    entity => entity.Course,
                    entity => entity.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Section was not found.");
            access.ReadSection(entity);

            var names = await users.GetNames([entity.InstructorId], cancellationToken);

            return entity.ToDTO(names.GetValueOrDefault(entity.InstructorId, "Instructor"));
        }
    }
}
