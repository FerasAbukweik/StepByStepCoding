using StepByStepCoding.Security.Services;
using Microsoft.Extensions.Logging;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Application.Common;

namespace StepByStepCoding.Security.Decorators
{
    public sealed class TokensServiceLoggingDecorator : LoggingDecorator, ITokensService
    {
        private readonly ITokensService inner;

        public TokensServiceLoggingDecorator(
            ITokensService inner,
            ILogger<TokensServiceLoggingDecorator> logger) : base(logger)
        {
            this.inner = inner;
        }

        public Task<TokensDTO> GenerateAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return LogOperation(
                $"{nameof(TokensService)}.{nameof(TokensService.GenerateAsync)}",
                () => inner.GenerateAsync(userId, cancellationToken),
                cancellationToken: cancellationToken);
        }

        public Task<Result<TokensDTO>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return LogOperation(
                $"{nameof(TokensService)}.{nameof(TokensService.RefreshAsync)}",
                () => inner.RefreshAsync(refreshToken, cancellationToken),
                isSuccessful: result => result.IsSuccess,
                cancellationToken: cancellationToken);
        }
    }
}
