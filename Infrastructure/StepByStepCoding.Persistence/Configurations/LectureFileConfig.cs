using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Persistence.Configurations
{
    internal sealed class LectureFileConfig : IEntityTypeConfiguration<LectureFile>
    {
        public void Configure(EntityTypeBuilder<LectureFile> builder)
        {
            builder.ToTable("LectureFiles");
            builder.HasIndex(entity => entity.LectureId).IsUnique();

            builder.HasOne(entity => entity.Lecture).WithOne(entity => entity.File)
                .HasForeignKey<LectureFile>(entity => entity.LectureId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
