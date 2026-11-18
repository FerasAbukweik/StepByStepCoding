using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Persistence.Configurations
{
    internal sealed class EnrollmentConfig : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.ToTable("Enrollments");
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsConcurrencyToken();
            builder.Ignore(entity => entity.OriginalVersion);

            builder.HasIndex(entity => new { entity.SectionId, entity.StudentId }).IsUnique();
            builder.HasIndex(entity => entity.StudentId);

            builder.HasOne(entity => entity.Section).WithMany(entity => entity.Enrollments)
                .HasForeignKey(entity => entity.SectionId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
