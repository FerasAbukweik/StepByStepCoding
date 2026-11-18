using System.Net;
using StepByStepCoding.Application.Contracts.Persistence;
using Microsoft.AspNetCore.Identity;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Application.Common;
using StepByStepCoding.Security.Models;

namespace StepByStepCoding.Security.Services
{
    public sealed class TokensService : ITokensService
    {
        private readonly IAccessTokenService accessTokens;
        private readonly IRefreshTokenService refreshTokens;
        private readonly IUnitOfWork<ApplicationSecurityDbContext> unitOfWork;
        private readonly UserManager<ApplicationUser> users;

        public TokensService(
            IAccessTokenService accessTokens,
            IRefreshTokenService refreshTokens,
            IUnitOfWork<ApplicationSecurityDbContext> unitOfWork,
            UserManager<ApplicationUser> users)
        {
            this.accessTokens = accessTokens;
            this.refreshTokens = refreshTokens;
            this.unitOfWork = unitOfWork;
            this.users = users;
        }

        public async Task<TokensDTO> GenerateAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var access = await accessTokens.GenerateAsync(userId);
            var refresh = await refreshTokens.CreateAsync(userId, cancellationToken);

            return new TokensDTO
            {
                AccessToken = access.AccessToken,
                AccessTokenExpiresAt = access.AccessTokenExpiresAt,
                RefreshToken = refresh.RefreshToken,
                RefreshTokenExpiresAt = refresh.RefreshTokenExpiresAt
            };
        }

        public Task<Result<TokensDTO>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return unitOfWork.ExecuteInTransaction(async token =>
            {
                var userId = await refreshTokens.ConsumeAsync(refreshToken, token);
                var user = userId.HasValue ? await users.FindByIdAsync(userId.Value.ToString()) : null;

                if (user is null || await users.IsLockedOutAsync(user))
                    return Result<TokensDTO>.Failure("Invalid or expired refresh token.", HttpStatusCode.Unauthorized);

                var tokens = await GenerateAsync(user.Id, token);

                return Result<TokensDTO>.Success(tokens);
            }, shouldCommit: result => result.IsSuccess, cancellationToken: cancellationToken);
        }
    }
}
