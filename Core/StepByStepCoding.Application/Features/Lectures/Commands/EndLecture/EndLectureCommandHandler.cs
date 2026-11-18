using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Contracts.Realtime;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Features.Common;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Lectures.Commands.EndLecture
{
    public sealed class EndLectureCommandHandler : IRequestHandler<EndLectureCommand>
    {
        private readonly ILectureRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ISignalRService signalR;
        private readonly IAcademicAccess access;
        private readonly TimeProvider clock;

        public EndLectureCommandHandler(
            ILectureRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            TimeProvider clock,
            ISignalRService signalR)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.signalR = signalR;
            this.access = access;
            this.clock = clock;
        }

        public async Task Handle(EndLectureCommand request, CancellationToken cancellationToken)
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
            entity.End(clock.GetUtcNow());

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

            if (entity.File is not null)
                await signalR.LectureEnded(entity.Id, entity.File.Id);
        }
    }
}
