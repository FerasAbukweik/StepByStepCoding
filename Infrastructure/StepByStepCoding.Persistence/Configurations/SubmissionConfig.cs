using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Persistence.Configurations
{
    internal sealed class SubmissionConfig : IEntityTypeConfiguration<Submission>
    {
        public void Configure(EntityTypeBuilder<Submission> builder)
        {
            builder.ToTable("Submissions");
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsConcurrencyToken();
            builder.Ignore(entity => entity.OriginalVersion);

            builder.Property(entity => entity.SubmittedCode).HasMaxLength(100000).IsRequired();
            builder.Property(entity => entity.Score).HasPrecision(18, 2);
            builder.Property(entity => entity.Feedback).HasMaxLength(8000);

            builder.HasIndex(entity => new { entity.AttemptId, entity.QuestionId }).IsUnique();
            builder.HasIndex(entity => entity.ReviewedBy);

            builder.HasOne(entity => entity.Attempt).WithMany(entity => entity.Submissions)
                .HasForeignKey(entity => entity.AttemptId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(entity => entity.Question).WithMany(entity => entity.Submissions)
                .HasForeignKey(entity => entity.QuestionId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
