using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepByStepCoding.Security.Models;

namespace StepByStepCoding.Security.Configurations
{
    internal sealed class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(user => user.FullName).HasMaxLength(250).IsRequired();
        }
    }
}
