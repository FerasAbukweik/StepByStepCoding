using System.Linq.Expressions;
using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Domain.Common;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentsList;
using StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentDetail;
using StepByStepCoding.Application.Features.Sections.Queries.GetSectionDetail;
using StepByStepCoding.Application.Features.Sections.Queries.GetSectionsList;
using StepByStepCoding.Application.Features.Courses.Queries.GetCoursesList;
using StepByStepCoding.Application.Features.Courses.Queries.GetCourseDetail;
using StepByStepCoding.Application.Features.Files.Queries.GetLectureFilesList;
using StepByStepCoding.Application.Features.Files.Queries.GetLectureFileId;
using StepByStepCoding.Application.Features.Files.Queries.GetLectureFileByLecture;
using StepByStepCoding.Application.Features.Files.Queries.GetFileDetail;
using StepByStepCoding.Application.Features.Submissions.Queries.GetSubmissionsList;
using StepByStepCoding.Application.Features.Submissions.Queries.GetSubmissionDetail;
using StepByStepCoding.Application.Features.ExamAttempts.Queries.GetExamAttemptDetail;
using StepByStepCoding.Application.Features.ExamAttempts.Queries.GetExamAttemptsList;
using StepByStepCoding.Application.Features.Exams.Queries.GetExamsList;
using StepByStepCoding.Application.Features.Exams.Queries.GetExamDetail;
using StepByStepCoding.Application.Features.Lectures.Queries.GetLecturesList;
using StepByStepCoding.Application.Features.Lectures.Queries.GetLectureDetail;
using StepByStepCoding.Application.Features.Files.Queries.GetCodeSnapshot;
using StepByStepCoding.Application.Features.ExamQuestions.Queries.GetExamQuestionsList;
using StepByStepCoding.Application.Features.ExamQuestions.Queries.GetExamQuestionDetail;

namespace StepByStepCoding.Application.Services
{
    public sealed class AcademicCachePolicy
    {
        private readonly IAcademicAccess access;
        private readonly ISectionRepository sections;
        private readonly IEnrollmentRepository enrollments;
        private readonly ILectureRepository lectures;
        private readonly ILectureFileRepository files;
        private readonly IFileRepository codeFiles;
        private readonly IExamRepository exams;
        private readonly IExamQuestionRepository questions;
        private readonly IExamAttemptRepository attempts;
        private readonly ISubmissionRepository submissions;

        public AcademicCachePolicy(IAcademicAccess access,
            ISectionRepository sections,
            IEnrollmentRepository enrollments,
            ILectureRepository lectures,
            ILectureFileRepository files,
            IFileRepository codeFiles,
            IExamRepository exams,
            IExamQuestionRepository questions,
            IExamAttemptRepository attempts,
            ISubmissionRepository submissions)
        {
            this.access = access;
            this.sections = sections;
            this.enrollments = enrollments;
            this.lectures = lectures;
            this.files = files;
            this.codeFiles = codeFiles;
            this.exams = exams;
            this.questions = questions;
            this.attempts = attempts;
            this.submissions = submissions;
        }

        public static bool AppliesTo(object request)
        {
            var type = request.GetType();
            return type.Assembly == typeof(AcademicCachePolicy).Assembly
                && type.Namespace?.StartsWith("StepByStepCoding.Application.Features.", StringComparison.Ordinal) == true;
        }

        public static bool Bypass(object request)
        {
            // Snapshot reads use the file access path; question visibility changes when the exam starts.
            return request is GetCodeSnapshotQuery or GetExamQuestionsListQuery;
        }

        public AcademicReadScope CurrentScope() => access.CurrentScope();

        public async Task<bool> CanUse<TResponse>(object request, TResponse response, CancellationToken cancellationToken)
        {
            var userId = access.CurrentScope().UserId;

            try
            {
                switch (request)
                {
                    case GetSectionDetailQuery value:
                        {
                            var entity = await Check(sections, value.Id, access.ReadSection,
                                [
                                    entity => entity.Enrollments
                                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                                ], cancellationToken);
                            return response is SectionDetailDTO detail && detail.Version == entity.Version;
                        }
                    case GetEnrollmentDetailQuery value:
                        {
                            var entity = await Check(enrollments, value.Id, access.ReadEnrollment, [entity => entity.Section], cancellationToken);
                            return response is EnrollmentDetailDTO detail && detail.Version == entity.Version;
                        }
                    case GetLectureDetailQuery value:
                        {
                            var entity = await Check(lectures, value.Id, access.ReadLecture,
                                [
                                    entity => entity.Section!.Enrollments
                                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active),
                                entity => entity.File
                                ], cancellationToken);
                            return response is LectureDetailDTO detail && detail.Version == entity.Version;
                        }
                    case GetFileDetailQuery value:
                        {
                            var entity = await codeFiles.GetWithEnrollment(value.Id, userId, cancellationToken)
                                ?? throw new NotFoundException("Lecture file was not found.");
                            access.ReadFile(entity);
                            return response is FileDetailDTO detail && detail.Version == entity.Version;
                        }
                    case GetExamDetailQuery value:
                        {
                            var entity = await Check(exams, value.Id, access.ReadExam,
                                [
                                    entity => entity.Section!.Enrollments
                                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                                ], cancellationToken);
                            return response is ExamDetailDTO detail && detail.Version == entity.Version;
                        }
                    case GetExamQuestionDetailQuery value:
                        {
                            var entity = await Check(questions, value.Id, access.ReadQuestion,
                                [
                                    entity => entity.Exam!.Section!.Enrollments
                                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                                ], cancellationToken);
                            return response is ExamQuestionDetailDTO detail && detail.Version == entity.Version;
                        }
                    case GetExamAttemptDetailQuery value:
                        {
                            var entity = await Check(attempts, value.Id, access.ReadAttempt,
                                [
                                    entity => entity.Exam!.Section!.Enrollments
                                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                                ], cancellationToken);
                            return response is ExamAttemptDetailDTO detail && detail.Version == entity.Version;
                        }
                    case GetSubmissionDetailQuery value:
                        {
                            var entity = await Check(submissions, value.Id, access.ReadSubmission,
                                [
                                    entity => entity.Attempt!.Exam!.Section!.Enrollments
                                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                                ], cancellationToken);
                            return response is SubmissionDetailDTO detail && detail.Version == entity.Version;
                        }
                    case GetLectureFileByLectureQuery value:
                        {
                            var file = await CheckFileByLecture(value.LectureId, userId, cancellationToken);
                            return response is FileDetailDTO detail && detail.Id == file.Id && detail.Version == file.Version;
                        }
                    case GetLectureFileIdQuery value:
                        {
                            var file = await CheckFileByLecture(value.LectureId, userId, cancellationToken);
                            return response is Guid id && id == file.Id;
                        }
                }

                return response switch
                {
                    PaginatedDTO<SectionListDTO> page => await Matches(page.Items.Select(item => (item.Id, item.Version)),
                        sections, access.ReadSection,
                            [
                                entity => entity.Enrollments
                                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                            ], cancellationToken),
                    PaginatedDTO<EnrollmentListDTO> page => await Matches(page.Items.Select(item => (item.Id, item.Version)),
                        enrollments, access.ReadEnrollment, [entity => entity.Section], cancellationToken),
                    PaginatedDTO<LectureListDTO> page => await Matches(page.Items.Select(item => (item.Id, item.Version)),
                        lectures, access.ReadLecture,
                            [
                                entity => entity.Section!.Enrollments
                                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active),
                                entity => entity.File
                            ], cancellationToken),
                    PaginatedDTO<LectureFileListDTO> page => await Matches(page.Items.Select(item => (item.Id, item.Version)),
                        files, access.ReadFile,
                            [
                                entity => entity.Lecture!.Section!.Enrollments
                                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                            ], cancellationToken),
                    PaginatedDTO<ExamListDTO> page => await Matches(page.Items.Select(item => (item.Id, item.Version)),
                        exams, access.ReadExam,
                            [
                                entity => entity.Section!.Enrollments
                                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                            ], cancellationToken),
                    PaginatedDTO<ExamQuestionListDTO> page => await Matches(page.Items.Select(item => (item.Id, item.Version)),
                        questions, access.ReadQuestion,
                            [
                                entity => entity.Exam!.Section!.Enrollments
                                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                            ], cancellationToken),
                    PaginatedDTO<ExamAttemptListDTO> page => await Matches(page.Items.Select(item => (item.Id, item.Version)),
                        attempts, access.ReadAttempt,
                            [
                                entity => entity.Exam!.Section!.Enrollments
                                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                            ], cancellationToken),
                    PaginatedDTO<SubmissionListDTO> page => await Matches(page.Items.Select(item => (item.Id, item.Version)),
                        submissions, access.ReadSubmission,
                            [
                                entity => entity.Attempt!.Exam!.Section!.Enrollments
                                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                            ], cancellationToken),
                    _ => true
                };
            }
            catch (Exception exception) when (exception is ForbiddenException or NotFoundException)
            {
                return false;
            }
        }

        private static async Task<T> Check<T>(IRepository<T> repository, Guid id, Action<T> check,
            Expression<Func<T, object?>>[] include, CancellationToken cancellationToken) where T : AcademicEntity
        {
            var entity = await repository.GetById(id, include, cancellationToken)
                ?? throw new NotFoundException("The requested record was not found.");
            check(entity);

            return entity;
        }

        private async Task<LectureFile> CheckFileByLecture(Guid lectureId, Guid userId, CancellationToken cancellationToken)
        {
            var lecture = await Check(lectures, lectureId, access.ReadLecture,
                [
                    entity => entity.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active),
                    entity => entity.File
                ], cancellationToken);
            var file = lecture.File ?? throw new NotFoundException("Lecture file was not found.");
            access.ReadFile(file);

            return file;
        }

        private static async Task<bool> Matches<T>(IEnumerable<(Guid Id, Guid Version)> rows,
            IRepository<T> repository, Action<T> check, Expression<Func<T, object?>>[] include,
            CancellationToken cancellationToken) where T : AcademicEntity
        {
            var expected = rows.ToArray();
            if (expected.Length == 0)
                return true;

            var ids = expected.Select(row => row.Id).ToArray();
            var query = repository.Filter(entity => ids.Contains(entity.Id), include);
            var current = new Dictionary<Guid, T>();

            // EF queries support standard async enumeration; in-memory repositories use the fallback.
            if (query is IAsyncEnumerable<T> asynchronous)
            {
                await foreach (var entity in asynchronous.WithCancellation(cancellationToken))
                    current[entity.Id] = entity;
            }
            else
            {
                foreach (var entity in query)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    current[entity.Id] = entity;
                }
            }

            foreach (var row in expected)
            {
                if (!current.TryGetValue(row.Id, out var entity) || entity.Version != row.Version)
                    return false;

                check(entity);
            }

            return true;
        }
    }
}
