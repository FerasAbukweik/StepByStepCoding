using StepByStepCoding.Domain.Common;

namespace StepByStepCoding.Domain.Entities
{
    public sealed class ExamQuestion : AcademicEntity
    {
        public Guid ExamId
        {
            get; private set;
        }

        public string Title { get; private set; } = null!;

        public string Description { get; private set; } = null!;

        public string? ImagePath
        {
            get; private set;
        }

        public decimal MaxScore
        {
            get; private set;
        }

        public int DisplayOrder
        {
            get; private set;
        }

        public Exam? Exam
        {
            get; private set;
        }

        public ICollection<Submission> Submissions { get; private set; } = [];

        private ExamQuestion()
        {
        }

        public ExamQuestion(Guid examId, string title, string description, string? imagePath, decimal maxScore, int displayOrder)
        {
            ExamId = AcademicRules.Reference(examId, nameof(ExamId));
            Apply(title, description, imagePath, maxScore, displayOrder);
        }

        public void Update(string title, string description, string? imagePath, decimal maxScore, int displayOrder)
        {
            Apply(title, description, imagePath, maxScore, displayOrder);
            MarkChanged();
        }

        private void Apply(string title, string description, string? imagePath, decimal maxScore, int displayOrder)
        {
            AcademicRules.Score(maxScore);
            AcademicRules.Require(displayOrder >= 0 && displayOrder <= 1000, "Display order must be between 0 and 1000.");
            Title = AcademicRules.Text(title, nameof(Title), 250);
            Description = AcademicRules.Text(description, nameof(Description), 20000);
            ImagePath = AcademicRules.OptionalText(imagePath, nameof(ImagePath), 1024);
            MaxScore = maxScore;
            DisplayOrder = displayOrder;
        }
    }
}
