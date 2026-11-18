using Microsoft.Extensions.Logging;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Security.Services;

namespace StepByStepCoding.Security.Decorators
{
    public sealed class AccountServiceLoggingDecorator : LoggingDecorator, IAccountService
    {
        private readonly IAccountService inner;

        public AccountServiceLoggingDecorator(IAccountService inner, ILogger<AccountServiceLoggingDecorator> logger)
            : base(logger, isBoundary: true)
        {
            this.inner = inner;
        }

        public Task<UserDTO> Me(CancellationToken cancellationToken = default)
            => LogOperation($"{nameof(AccountService)}.{nameof(AccountService.Me)}", () => inner.Me(cancellationToken),
                cancellationToken: cancellationToken);

        public Task Logout(CancellationToken cancellationToken = default)
            => LogOperation($"{nameof(AccountService)}.{nameof(AccountService.Logout)}", async () =>
            {
                await inner.Logout(cancellationToken);
                return true;
            }, cancellationToken: cancellationToken);

        public Task ChangePassword(ChangePasswordDTO request, CancellationToken cancellationToken = default)
            => LogOperation($"{nameof(AccountService)}.{nameof(AccountService.ChangePassword)}", async () =>
            {
                await inner.ChangePassword(request, cancellationToken);
                return true;
            }, cancellationToken: cancellationToken);
    }
}
