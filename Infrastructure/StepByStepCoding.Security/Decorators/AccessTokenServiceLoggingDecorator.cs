using StepByStepCoding.Security.Services;
using Microsoft.Extensions.Logging;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;

namespace StepByStepCoding.Security.Decorators
{
    public sealed class AccessTokenServiceLoggingDecorator : LoggingDecorator, IAccessTokenService
    {
        private readonly IAccessTokenService inner;

        public AccessTokenServiceLoggingDecorator(
            IAccessTokenService inner,
            ILogger<AccessTokenServiceLoggingDecorator> logger) : base(logger)
        {
            this.inner = inner;
        }

        public Task<AccessTokenResultDTO> GenerateAsync(Guid userId)
        {
            return LogOperation(
                $"{nameof(AccessTokenService)}.{nameof(AccessTokenService.GenerateAsync)}",
                () => inner.GenerateAsync(userId));
        }
    }
}
