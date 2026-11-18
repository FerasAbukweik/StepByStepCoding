using StepByStepCoding.Security.Contracts.Repositories;
using System.Security.Cryptography;
using System.Text;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Security.Models;
using Microsoft.Extensions.Options;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Security.Options;

namespace StepByStepCoding.Security.Services
{
    public sealed class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository repository;
        private readonly IOptions<JwtOptions> options;

        public RefreshTokenService(IRefreshTokenRepository repository, IOptions<JwtOptions> options)
        {
            this.repository = repository;
            this.options = options;
        }

        public async Task<RefreshTokenResultDTO> CreateAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var now = DateTimeOffset.UtcNow;
            var expires = now.AddMinutes(options.Value.RefreshTokenLifeTime);

            // Persist only the hash; the raw token is returned to the client once.
            await repository.Add(new RefreshToken(userId, Hash(token), now, expires));

            return new RefreshTokenResultDTO
            {
                RefreshToken = token,
                RefreshTokenExpiresAt = expires
            };
        }

        public async Task<Guid?> ConsumeAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token) || token.Length > 128)
                return null;

            var refreshToken = await repository.Consume(Hash(token), DateTimeOffset.UtcNow, cancellationToken);

            return refreshToken?.UserId;
        }

        private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
