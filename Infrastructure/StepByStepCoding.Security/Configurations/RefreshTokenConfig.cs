using StepByStepCoding.Security.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StepByStepCoding.Security.Configurations
{
    internal sealed class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(token => token.Id);
            builder.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();

            builder.HasIndex(token => token.TokenHash).IsUnique();
            builder.HasIndex(token => token.UserId);
            builder.HasIndex(token => token.ExpiresAt);

            builder.HasOne<ApplicationUser>().WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
