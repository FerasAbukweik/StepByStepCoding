using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Persistence.Configurations
{
    internal sealed class ExamAttemptConfig : IEntityTypeConfiguration<ExamAttempt>
    {
        public void Configure(EntityTypeBuilder<ExamAttempt> builder)
        {
            builder.ToTable("ExamAttempts");
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsConcurrencyToken();
            builder.Ignore(entity => entity.OriginalVersion);

            builder.HasIndex(entity => new { entity.ExamId, entity.StudentId }).IsUnique();
            builder.HasIndex(entity => entity.StudentId);

            builder.HasOne(entity => entity.Exam).WithMany(entity => entity.Attempts)
                .HasForeignKey(entity => entity.ExamId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
