using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Persistence.Configurations
{
    internal sealed class ExamQuestionConfig : IEntityTypeConfiguration<ExamQuestion>
    {
        public void Configure(EntityTypeBuilder<ExamQuestion> builder)
        {
            builder.ToTable("ExamQuestions");
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsConcurrencyToken();
            builder.Ignore(entity => entity.OriginalVersion);

            builder.Property(entity => entity.Title).HasMaxLength(250).IsRequired();
            builder.Property(entity => entity.Description).HasMaxLength(20000).IsRequired();
            builder.Property(entity => entity.ImagePath).HasMaxLength(1024);
            builder.Property(entity => entity.MaxScore).HasPrecision(18, 2);

            builder.HasIndex(entity => new { entity.ExamId, entity.DisplayOrder }).IsUnique();

            builder.HasOne(entity => entity.Exam).WithMany(entity => entity.Questions)
                .HasForeignKey(entity => entity.ExamId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
