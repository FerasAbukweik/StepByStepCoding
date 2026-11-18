using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Persistence.Configurations
{
    internal sealed class ExamConfig : IEntityTypeConfiguration<Exam>
    {
        public void Configure(EntityTypeBuilder<Exam> builder)
        {
            builder.ToTable("Exams");
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsConcurrencyToken();
            builder.Ignore(entity => entity.OriginalVersion);

            builder.Property(entity => entity.Title).HasMaxLength(250).IsRequired();
            builder.Property(entity => entity.Instructions).HasMaxLength(8000);

            builder.HasIndex(entity => new { entity.SectionId, entity.StartsAt });

            builder.HasOne(entity => entity.Section).WithMany(entity => entity.Exams)
                .HasForeignKey(entity => entity.SectionId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
