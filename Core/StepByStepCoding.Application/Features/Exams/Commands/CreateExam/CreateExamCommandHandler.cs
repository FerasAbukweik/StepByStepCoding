using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Features.Common;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Domain.Common;

namespace StepByStepCoding.Application.Features.Exams.Commands.CreateExam
{
    public sealed class CreateExamCommandHandler : IRequestHandler<CreateExamCommand, Guid>
    {
        private readonly IExamRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly ISectionRepository sectionRepository;

        public CreateExamCommandHandler(
            IExamRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            ISectionRepository sectionRepository)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
            this.sectionRepository = sectionRepository;
        }

        public async Task<Guid> Handle(CreateExamCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var sectionForAccess = await sectionRepository.GetById(request.SectionId,
                [
                    entity => entity.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Section was not found.");
            access.ManageSection(sectionForAccess);

            var entity = new Exam(request.SectionId, request.Title, request.Instructions, request.StartsAt, request.Deadline, request.DurationMinutes);

            try
            {
                await repository.Add(entity);
                await unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                await unitOfWork.Rollback(CancellationToken.None);
                throw;
            }

            return entity.Id;
        }
    }
}
