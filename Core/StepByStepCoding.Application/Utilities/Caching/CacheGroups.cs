namespace StepByStepCoding.Application.Utilities.Caching
{
    public static class CacheGroups
    {
        public const string File = "file";

        public const string Academic = "academic";

        public const string Courses = Academic + ":courses";

        public const string Sections = Academic + ":sections";

        public const string Enrollments = Academic + ":enrollments";

        public const string Lectures = Academic + ":lectures";

        public const string LectureFiles = Academic + ":lecture-files";

        public const string Exams = Academic + ":exams";

        public const string ExamQuestions = Academic + ":exam-questions";

        public const string ExamAttempts = Academic + ":exam-attempts";

        public const string Submissions = Academic + ":submissions";
    }
}
