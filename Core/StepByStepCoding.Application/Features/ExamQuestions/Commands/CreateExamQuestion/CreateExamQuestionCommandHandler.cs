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

namespace StepByStepCoding.Application.Features.ExamQuestions.Commands.CreateExamQuestion
{
    public sealed class CreateExamQuestionCommandHandler : IRequestHandler<CreateExamQuestionCommand, Guid>
    {
        private readonly IExamQuestionRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAcademicAccess access;
        private readonly IExamRepository exams;

        public CreateExamQuestionCommandHandler(
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

        public async Task<Guid> Handle(CreateExamQuestionCommand request, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            var exam = await exams.GetById(request.ExamId,
                [
                    entity => entity.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken)
                ?? throw new NotFoundException("Exam was not found.");
            access.ManageExam(exam);
            exam.RegisterQuestionChange();

            if (await repository.Any(existing => existing.ExamId == request.ExamId && existing.DisplayOrder == request.DisplayOrder, cancellationToken))
                throw new ConflictException("A record with these details already exists.");

            var entity = new ExamQuestion(request.ExamId, request.Title, request.Description, request.ImagePath, request.MaxScore, request.DisplayOrder);

            try
            {
                await exams.Update(exam);
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
