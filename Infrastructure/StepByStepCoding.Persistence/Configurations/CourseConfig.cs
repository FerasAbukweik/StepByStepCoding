using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Persistence.Configurations
{
    internal sealed class CourseConfig : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable("Courses");
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsConcurrencyToken();
            builder.Ignore(entity => entity.OriginalVersion);

            builder.Property(entity => entity.Name).HasMaxLength(150).IsRequired();

            builder.HasIndex(entity => entity.Name).IsUnique();
        }
    }
}
