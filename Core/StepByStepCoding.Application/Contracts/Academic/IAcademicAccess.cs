using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Contracts.Academic
{
    public interface IAcademicAccess
    {
        AcademicReadScope CurrentScope();

        void ManageSection(Section section);
        void ReadSection(Section section);
        void ReadEnrollment(Enrollment enrollment);
        void ManageLecture(Lecture lecture);
        void ReadLecture(Lecture lecture);
        void ManageFile(CodeFile file);
        void ReadFile(CodeFile file);
        void ManageExam(Exam exam);
        void ReadExam(Exam exam);
        void ReadQuestion(ExamQuestion question);
        void ReadAttempt(ExamAttempt attempt);
        void ReadSubmission(Submission submission);
        void ManageSubmission(Submission submission);
    }
}
