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

namespace StepByStepCoding.Application.Features.Exams.Commands.CloseExam
{
    public sealed class CloseExamCommandHandler : IRequestHandler<CloseExamCommand>
    {
        private readonly IExamRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly TimeProvider clock;

        public CloseExamCommandHandler(
            IExamRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            TimeProvider clock)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
            this.clock = clock;
        }

        public async Task Handle(CloseExamCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await repository.GetById(request.Id,
                [
                    entity => entity.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Exam was not found.");
            access.ManageExam(entity);
            CommandChecks.Version(entity, request.Version);
            entity.Close();

            try
            {
                await repository.Update(entity);
                await unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                await unitOfWork.Rollback(CancellationToken.None);
                throw;
            }

        }
    }
}
