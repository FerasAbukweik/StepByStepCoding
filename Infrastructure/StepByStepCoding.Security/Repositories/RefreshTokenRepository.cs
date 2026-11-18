using StepByStepCoding.Security.Contracts.Repositories;
using StepByStepCoding.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Security.Models;

namespace StepByStepCoding.Security.Repositories
{
    public sealed class RefreshTokenRepository : Repository<RefreshToken, ApplicationSecurityDbContext>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationSecurityDbContext context) : base(context)
        {
        }

        public Task<int> DeleteForUser(Guid userId, CancellationToken cancellationToken = default)
        {
            return context.RefreshTokens.Where(token => token.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        }

        public Task<int> DeleteExpired(DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            return context.RefreshTokens.Where(token => token.ExpiresAt <= now)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<RefreshToken?> Consume(string tokenHash, DateTimeOffset now,
            CancellationToken cancellationToken = default)
        {
            var query = context.RefreshTokens.Where(token => token.TokenHash == tokenHash && token.ExpiresAt > now);
            var refreshToken = await query.AsNoTracking().SingleOrDefaultAsync(cancellationToken);

            // A conditional delete lets only one concurrent refresh request consume this token.
            return await query.ExecuteDeleteAsync(cancellationToken) == 1 ? refreshToken : null;
        }
    }
}
