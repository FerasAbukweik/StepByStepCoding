using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Persistence.Configurations
{
    internal sealed class WorkspaceFileConfig : IEntityTypeConfiguration<WorkspaceFile>
    {
        public void Configure(EntityTypeBuilder<WorkspaceFile> builder)
        {
            builder.ToTable("WorkspaceFiles");
            // Accounts live in the security database; UserId is the shared account identifier.
            builder.HasIndex(entity => entity.UserId).IsUnique();
        }
    }
}
