using StepByStepCoding.Security.Services;
using Microsoft.Extensions.Logging;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Application.Common;

namespace StepByStepCoding.Security.Decorators
{
    public sealed class AuthServiceLoggingDecorator : LoggingDecorator, IAuthService
    {
        private readonly IAuthService inner;

        public AuthServiceLoggingDecorator(
            IAuthService inner,
            ILogger<AuthServiceLoggingDecorator> logger) : base(logger, isBoundary: true)
        {
            this.inner = inner;
        }

        public Task<Result> SignupAsync(SignupDTO request, CancellationToken cancellationToken = default)
        {
            return LogOperation(
                $"{nameof(AuthService)}.{nameof(AuthService.SignupAsync)}",
                () => inner.SignupAsync(request, cancellationToken),
                isSuccessful: result => result.IsSuccess,
                cancellationToken: cancellationToken);
        }

        public Task<Result<TokensDTO>> LoginAsync(LoginDTO request, CancellationToken cancellationToken = default)
        {
            return LogOperation(
                $"{nameof(AuthService)}.{nameof(AuthService.LoginAsync)}",
                () => inner.LoginAsync(request, cancellationToken),
                isSuccessful: result => result.IsSuccess,
                cancellationToken: cancellationToken);
        }

        public Task<Result<TokensDTO>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return LogOperation(
                $"{nameof(AuthService)}.{nameof(AuthService.RefreshTokenAsync)}",
                () => inner.RefreshTokenAsync(refreshToken, cancellationToken),
                isSuccessful: result => result.IsSuccess,
                cancellationToken: cancellationToken);
        }
    }
}
