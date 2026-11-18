using StepByStepCoding.Security.Services;
using Microsoft.Extensions.Logging;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;

namespace StepByStepCoding.Security.Decorators
{
    public sealed class RefreshTokenServiceLoggingDecorator : LoggingDecorator, IRefreshTokenService
    {
        private readonly IRefreshTokenService inner;

        public RefreshTokenServiceLoggingDecorator(
            IRefreshTokenService inner,
            ILogger<RefreshTokenServiceLoggingDecorator> logger) : base(logger)
        {
            this.inner = inner;
        }

        public Task<RefreshTokenResultDTO> CreateAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return LogOperation(
                $"{nameof(RefreshTokenService)}.{nameof(RefreshTokenService.CreateAsync)}",
                () => inner.CreateAsync(userId, cancellationToken),
                cancellationToken: cancellationToken);
        }

        public Task<Guid?> ConsumeAsync(string token, CancellationToken cancellationToken = default)
        {
            return LogOperation(
                $"{nameof(RefreshTokenService)}.{nameof(RefreshTokenService.ConsumeAsync)}",
                () => inner.ConsumeAsync(token, cancellationToken),
                isSuccessful: userId => userId.HasValue,
                cancellationToken: cancellationToken);
        }
    }
}
