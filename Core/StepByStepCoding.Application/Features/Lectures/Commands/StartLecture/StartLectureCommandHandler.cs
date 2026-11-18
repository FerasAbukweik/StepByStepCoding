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

namespace StepByStepCoding.Application.Features.Lectures.Commands.StartLecture
{
    public sealed class StartLectureCommandHandler : IRequestHandler<StartLectureCommand>
    {
        private readonly ILectureRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly TimeProvider clock;

        public StartLectureCommandHandler(
            ILectureRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            TimeProvider clock)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
            this.clock = clock;
        }

        public async Task Handle(StartLectureCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await repository.GetById(request.Id,
                [
                    entity => entity.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active),
                    entity => entity.File
                ], cancellationToken)
                ?? throw new NotFoundException("Lecture was not found.");
            access.ManageLecture(entity);
            CommandChecks.Version(entity, request.Version);
            entity.Start(clock.GetUtcNow());

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
