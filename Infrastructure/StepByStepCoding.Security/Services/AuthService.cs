using StepByStepCoding.Application.Enums;
using StepByStepCoding.Application.Exceptions;
using System.Net;
using StepByStepCoding.Application.Contracts.Persistence;
using Microsoft.AspNetCore.Identity;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Application.Common;
using StepByStepCoding.Security.Models;

namespace StepByStepCoding.Security.Services
{
    public sealed class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> users;
        private readonly ITokensService tokens;
        private readonly IUserService currentUser;
        private readonly RoleManager<ApplicationRole> roles;
        private readonly IUnitOfWork<ApplicationSecurityDbContext> unitOfWork;

        public AuthService(
            UserManager<ApplicationUser> users,
            ITokensService tokens,
            IUserService currentUser,
            RoleManager<ApplicationRole> roles,
            IUnitOfWork<ApplicationSecurityDbContext> unitOfWork)
        {
            this.users = users;
            this.tokens = tokens;
            this.currentUser = currentUser;
            this.roles = roles;
            this.unitOfWork = unitOfWork;
        }

        public Task<Result> SignupAsync(SignupDTO request, CancellationToken cancellationToken = default)
        {
            if (currentUser.GetUserId() is null)
                throw new UnauthenticatedException("Authentication is required to create an account.");

            if (request.Roles.Count == 0 || request.Roles.Any(role => !Enum.IsDefined(role)))
                return Task.FromResult(Result.Failure("At least one valid role is required."));

            var isAdmin = currentUser.IsInRole(nameof(UserRole.Admin));
            var isInstructor = currentUser.IsInRole(nameof(UserRole.Instructor));

            if (request.Roles.Any(role => role switch
            {
                UserRole.Instructor => !isAdmin,
                UserRole.Student => !isAdmin && !isInstructor,
                _ => true
            }))
                throw new ForbiddenException("You cannot assign one or more of the requested roles.");

            return Register(request, cancellationToken);
        }

        internal Task<Result> Register(SignupDTO request, CancellationToken cancellationToken)
        {
            var requestedRoles = request.Roles.Distinct().Select(role => role.ToString()).ToArray();

            // Identity writes immediately; account creation and role assignment must commit together.
            return unitOfWork.ExecuteInTransaction(async token =>
            {
                var user = new ApplicationUser
                {
                    Id = Guid.CreateVersion7(),
                    FullName = request.FullName.Trim(),
                    UserName = request.Email,
                    Email = request.Email
                };

                var result = await users.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                    return Result.Failure(string.Join(" ", result.Errors.Select(e => e.Description)), HttpStatusCode.BadRequest);

                foreach (var role in requestedRoles)
                {
                    if (await roles.RoleExistsAsync(role))
                        continue;

                    var roleResult = await roles.CreateAsync(new ApplicationRole { Name = role });

                    if (!roleResult.Succeeded)
                        return Result.Failure(string.Join(" ", roleResult.Errors.Select(error => error.Description)));
                }

                var assignment = await users.AddToRolesAsync(user, requestedRoles);

                if (!assignment.Succeeded)
                    return Result.Failure(string.Join(" ", assignment.Errors.Select(error => error.Description)));

                return Result.Success(HttpStatusCode.Created);
            }, shouldCommit: result => result.IsSuccess, cancellationToken: cancellationToken);
        }

        public async Task<Result<TokensDTO>> LoginAsync(LoginDTO request, CancellationToken cancellationToken = default)
        {
            var user = await users.FindByEmailAsync(request.Email);

            if (user is null || await users.IsLockedOutAsync(user))
                return InvalidCredentials();

            if (!await users.CheckPasswordAsync(user, request.Password))
            {
                await users.AccessFailedAsync(user);

                return InvalidCredentials();
            }

            await users.ResetAccessFailedCountAsync(user);

            var response = await tokens.GenerateAsync(user.Id, cancellationToken);
            await unitOfWork.Commit(cancellationToken);

            return Result<TokensDTO>.Success(response);
        }

        public Task<Result<TokensDTO>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
            => tokens.RefreshAsync(refreshToken, cancellationToken);

        private static Result<TokensDTO> InvalidCredentials()
            => Result<TokensDTO>.Failure("Invalid email or password.", HttpStatusCode.Unauthorized);
    }
}
