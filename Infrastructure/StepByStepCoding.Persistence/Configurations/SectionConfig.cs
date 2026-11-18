using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Persistence.Configurations
{
    internal sealed class SectionConfig : IEntityTypeConfiguration<Section>
    {
        public void Configure(EntityTypeBuilder<Section> builder)
        {
            builder.ToTable("Sections");
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsConcurrencyToken();
            builder.Ignore(entity => entity.OriginalVersion);

            builder.Property(entity => entity.Name).HasMaxLength(150).IsRequired();

            builder.HasIndex(entity => new { entity.CourseId, entity.Name }).IsUnique();
            builder.HasIndex(entity => entity.InstructorId);

            builder.HasOne(entity => entity.Course).WithMany(entity => entity.Sections)
                .HasForeignKey(entity => entity.CourseId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
