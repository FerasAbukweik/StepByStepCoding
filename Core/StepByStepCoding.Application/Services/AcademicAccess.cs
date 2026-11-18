using StepByStepCoding.Application.Enums;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Services
{
    public sealed class AcademicAccess : IAcademicAccess
    {
        private readonly IUserService user;
        private readonly TimeProvider clock;

        public AcademicAccess(IUserService user, TimeProvider clock)
        {
            this.user = user;
            this.clock = clock;
        }

        public AcademicReadScope CurrentScope()
        {
            var id = user.GetUserId();

            if (id is null || id == Guid.Empty)
                throw new UnauthenticatedException("Authentication is required.");

            return new(id.Value, user.IsInRole(nameof(UserRole.Admin)), user.IsInRole(nameof(UserRole.Instructor)),
                user.IsInRole(nameof(UserRole.Student)), clock.GetUtcNow());
        }

        public void ManageSection(Section section)
        {
            ArgumentNullException.ThrowIfNull(section);
            if (!CanManage(section))
                throw new ForbiddenException("You do not manage this section.");
        }

        public void ReadSection(Section section)
        {
            ArgumentNullException.ThrowIfNull(section);
            var scope = CurrentScope();

            if (!CanManage(section) && !(scope.IsStudent && section.Enrollments.Any(enrollment =>
                enrollment.StudentId == scope.UserId && enrollment.Status == EnrollmentStatus.Active)))
                throw new ForbiddenException("You are not enrolled in this section.");
        }

        public void ReadEnrollment(Enrollment enrollment)
        {
            var scope = CurrentScope();

            if (!(scope.IsStudent && enrollment.StudentId == scope.UserId))
                ManageSection(RequireLoadedRelation(enrollment.Section));
        }

        public void ManageLecture(Lecture lecture) => ManageSection(RequireLoadedRelation(lecture.Section));

        public void ReadLecture(Lecture lecture) => ReadSection(RequireLoadedRelation(lecture.Section));

        public void ManageFile(CodeFile file)
        {
            if (file is WorkspaceFile workspace)
            {
                if (workspace.UserId != CurrentScope().UserId)
                    throw new ForbiddenException("This file belongs to another user.");

                return;
            }

            var lecture = RequireLoadedRelation(((LectureFile)file).Lecture);
            ManageLecture(lecture);

            if (lecture.Status == LectureStatus.Ended)
                throw new ConflictException("Ended lecture files are read-only.");
        }

        public void ReadFile(CodeFile file)
        {
            if (file is WorkspaceFile workspace)
            {
                if (workspace.UserId != CurrentScope().UserId)
                    throw new ForbiddenException("This file belongs to another user.");

                return;
            }

            var lecture = RequireLoadedRelation(((LectureFile)file).Lecture);
            ReadLecture(lecture);

            if (lecture.Status == LectureStatus.Scheduled && !CanManage(RequireLoadedRelation(lecture.Section)))
                throw new ForbiddenException("Lecture files are not available before the lecture starts.");
        }

        public void ManageExam(Exam exam) => ManageSection(RequireLoadedRelation(exam.Section));

        public void ReadExam(Exam exam)
        {
            var section = RequireLoadedRelation(exam.Section);
            ReadSection(section);

            if (exam.Status == ExamStatus.Draft && !CanManage(section))
                throw new ForbiddenException("Draft exams are not available to students.");
        }

        public void ReadQuestion(ExamQuestion question)
        {
            var exam = RequireLoadedRelation(question.Exam);
            ReadExam(exam);

            if (!CanManage(RequireLoadedRelation(exam.Section)) && clock.GetUtcNow() < exam.StartsAt)
                throw new ForbiddenException("Exam questions are not available before the exam starts.");
        }

        public void ReadAttempt(ExamAttempt attempt)
        {
            var scope = CurrentScope();

            if (!(scope.IsStudent && attempt.StudentId == scope.UserId))
                ManageExam(RequireLoadedRelation(attempt.Exam));
        }

        public void ReadSubmission(Submission submission) => ReadAttempt(RequireLoadedRelation(submission.Attempt));

        public void ManageSubmission(Submission submission) => ManageExam(RequireLoadedRelation(RequireLoadedRelation(submission.Attempt).Exam));

        // Missing access relationships are a loading error, never a reason to allow access.
        private static T RequireLoadedRelation<T>(T? entity) where T : class
            => entity ?? throw new InvalidOperationException("Load the relationships required for the access check.");

        private bool CanManage(Section section)
        {
            var scope = CurrentScope();

            return scope.IsAdmin || (scope.IsInstructor && section.InstructorId == scope.UserId);
        }
    }
}
