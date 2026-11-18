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

namespace StepByStepCoding.Application.Features.ExamQuestions.Commands.DeleteExamQuestion
{
    public sealed class DeleteExamQuestionCommandHandler : IRequestHandler<DeleteExamQuestionCommand>
    {
        private readonly IExamQuestionRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly IExamRepository exams;

        public DeleteExamQuestionCommandHandler(
            IExamQuestionRepository repository,
            IUnitOfWork unitOfWork,
            IAcademicAccess access,
            IExamRepository exams)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.access = access;
            this.exams = exams;
        }

        public async Task Handle(DeleteExamQuestionCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var entity = await repository.GetById(request.Id,
                [
                    entity => entity.Exam!.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Question was not found.");
            var exam = entity.Exam!;
            access.ManageExam(exam);
            exam.RegisterQuestionChange();
            CommandChecks.Version(entity, request.Version);

            if (await repository.HasDependents(entity.Id, cancellationToken))
                throw new ConflictException("Remove related records before deleting this record.");

            try
            {
                await exams.Update(exam);
                await repository.Delete(entity);
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
