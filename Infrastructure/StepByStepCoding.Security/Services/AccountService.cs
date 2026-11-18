using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Application.Features.Files.Commands.EnsureWorkspaceFile;
using Microsoft.AspNetCore.Identity;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Security.Contracts.Repositories;
using StepByStepCoding.Security.Models;

namespace StepByStepCoding.Security.Services
{
    public sealed class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> users;
        private readonly IMediator mediator;
        private readonly IUserService currentUser;
        private readonly IRefreshTokenRepository refreshTokens;
        private readonly IUnitOfWork<ApplicationSecurityDbContext> unitOfWork;

        public AccountService(UserManager<ApplicationUser> users, IUserService currentUser, IMediator mediator,
            IRefreshTokenRepository refreshTokens, IUnitOfWork<ApplicationSecurityDbContext> unitOfWork)
        {
            this.users = users;
            this.mediator = mediator;
            this.currentUser = currentUser;
            this.refreshTokens = refreshTokens;
            this.unitOfWork = unitOfWork;
        }

        public async Task<UserDTO> Me(CancellationToken cancellationToken = default)
        {
            var user = await GetUser(cancellationToken);

            return new UserDTO
            {
                Id = user.Id,
                FileId = await mediator.Send(new EnsureWorkspaceFileCommand(), cancellationToken),
                FullName = user.FullName,
                Email = user.Email ?? "",
                Roles = (await users.GetRolesAsync(user)).ToArray()
            };
        }

        public async Task ChangePassword(ChangePasswordDTO request, CancellationToken cancellationToken = default)
        {
            await unitOfWork.ExecuteInTransaction(async token =>
            {
                var user = await GetUser(token);
                var result = await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                if (!result.Succeeded)
                    throw new CustomValidationException(string.Join(" ", result.Errors.Select(error => error.Description)));

                await refreshTokens.DeleteForUser(user.Id, token);

                return true;
            }, cancellationToken: cancellationToken);
        }

        public async Task Logout(CancellationToken cancellationToken = default)
        {
            var id = currentUser.GetUserId() ?? throw new UnauthenticatedException("Authentication is required.");
            await refreshTokens.DeleteForUser(id, cancellationToken);
        }

        private async Task<ApplicationUser> GetUser(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = currentUser.GetUserId() ?? throw new UnauthenticatedException("Authentication is required.");

            return await users.FindByIdAsync(id.ToString())
                ?? throw new UnauthenticatedException("This account is no longer available.");
        }
    }
}
