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

namespace StepByStepCoding.Application.Features.ExamQuestions.Commands.UpdateExamQuestion
{
    public sealed class UpdateExamQuestionCommandHandler : IRequestHandler<UpdateExamQuestionCommand>
    {
        private readonly IExamQuestionRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly IExamRepository exams;

        public UpdateExamQuestionCommandHandler(
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

        public async Task Handle(UpdateExamQuestionCommand request, CancellationToken cancellationToken)
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
            var current = entity;

            if (await repository.Any(existing => existing.Id != request.Id && existing.ExamId == current.ExamId && existing.DisplayOrder == request.DisplayOrder, cancellationToken))
                throw new ConflictException("A record with these details already exists.");

            entity.Update(request.Title, request.Description, request.ImagePath, request.MaxScore, request.DisplayOrder);

            try
            {
                await exams.Update(exam);
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
