using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Persistence.Configurations
{
    internal sealed class LectureConfig : IEntityTypeConfiguration<Lecture>
    {
        public void Configure(EntityTypeBuilder<Lecture> builder)
        {
            builder.ToTable("Lectures");
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsConcurrencyToken();
            builder.Ignore(entity => entity.OriginalVersion);

            builder.Property(entity => entity.Title).HasMaxLength(250).IsRequired();

            builder.HasIndex(entity => new { entity.SectionId, entity.ScheduledStart });

            builder.HasOne(entity => entity.Section).WithMany(entity => entity.Lectures)
                .HasForeignKey(entity => entity.SectionId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
