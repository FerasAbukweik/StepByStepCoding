using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Persistence.Configurations
{
    internal sealed class CodeFileConfig : IEntityTypeConfiguration<CodeFile>
    {
        public void Configure(EntityTypeBuilder<CodeFile> builder)
        {
            builder.UseTpcMappingStrategy();
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsConcurrencyToken();
            builder.Ignore(entity => entity.OriginalVersion);
            builder.Property(entity => entity.FileName).HasMaxLength(250).IsRequired();
            builder.Property(entity => entity.SavedContent).HasMaxLength(100000).IsRequired();
        }
    }
}
