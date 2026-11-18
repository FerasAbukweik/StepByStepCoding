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

namespace StepByStepCoding.Application.Features.ExamAttempts.Commands.StartExamAttempt
{
    public sealed class StartExamAttemptCommandHandler : IRequestHandler<StartExamAttemptCommand, Guid>
    {
        private readonly IExamAttemptRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly IExamRepository examRepository;
        private readonly TimeProvider clock;

        public StartExamAttemptCommandHandler(
            IExamAttemptRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            TimeProvider clock,
            IExamRepository examRepository)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
            this.examRepository = examRepository;
            this.clock = clock;
        }

        public async Task<Guid> Handle(StartExamAttemptCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var scope = access.CurrentScope();

            var studentId = scope.UserId;
            var exam = await examRepository.GetById(request.ExamId,
                [
                    entity => entity.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Exam was not found.");
            access.ReadExam(exam);
            var now = clock.GetUtcNow();
            AcademicRules.Require(exam.Status == ExamStatus.Published && now >= exam.StartsAt && now < exam.Deadline,
                "The exam is not open for attempts.");

            if (await repository.Any(existing => existing.ExamId == request.ExamId && existing.StudentId == studentId, cancellationToken))
                throw new ConflictException("You already have an attempt for this exam.");

            var expiresAt = now.AddMinutes(exam.DurationMinutes);

            if (expiresAt > exam.Deadline)
                expiresAt = exam.Deadline;

            var entity = new ExamAttempt(exam.Id, studentId, now, expiresAt);

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
