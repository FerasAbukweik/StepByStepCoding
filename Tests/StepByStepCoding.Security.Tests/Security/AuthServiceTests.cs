using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StepByStepCoding.Application.Common;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Enums;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Security.Models;
using StepByStepCoding.Security.Services;

namespace StepByStepCoding.Security.Tests.Security
{
    public sealed class AuthServiceTests : TestBase
    {
        private readonly Mock<UserManager<ApplicationUser>> users;
        private readonly Mock<RoleManager<ApplicationRole>> roles;
        private readonly Mock<ITokensService> tokens = new();
        private readonly Mock<IUserService> currentUser = new();
        private readonly Mock<IUnitOfWork<ApplicationSecurityDbContext>> unitOfWork = new();
        private readonly AuthService service;

        public AuthServiceTests(ITestOutputHelper output) : base(output)
        {
            users = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                Microsoft.Extensions.Options.Options.Create(new IdentityOptions()), new Mock<IPasswordHasher<ApplicationUser>>().Object,
                Array.Empty<IUserValidator<ApplicationUser>>(), Array.Empty<IPasswordValidator<ApplicationUser>>(),
                new Mock<ILookupNormalizer>().Object, new IdentityErrorDescriber(),
                new Mock<IServiceProvider>().Object, new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
            roles = new Mock<RoleManager<ApplicationRole>>(
                new Mock<IRoleStore<ApplicationRole>>().Object, Array.Empty<IRoleValidator<ApplicationRole>>(),
                new Mock<ILookupNormalizer>().Object, new IdentityErrorDescriber(),
                new Mock<ILogger<RoleManager<ApplicationRole>>>().Object);
            service = new AuthService(users.Object, tokens.Object, currentUser.Object, roles.Object, unitOfWork.Object);
        }

        #region Login

        [Theory]
        [InlineData("missing")]
        [InlineData("locked")]
        [InlineData("wrong-password")]
        [InlineData("success")]
        public async Task Login_IssuesTokensOnlyForValidUnlockedAccount(string scenario)
        {
            // Arrange
            var request = Fixture.Create<LoginDTO>();
            var user = Fixture.Build<ApplicationUser>().OmitAutoProperties().With(entity => entity.Id, Fixture.Create<Guid>()).Create();
            var expected = Fixture.Create<TokensDTO>();
            users.Setup(manager => manager.FindByEmailAsync(request.Email)).ReturnsAsync(scenario == "missing" ? null : user);
            users.Setup(manager => manager.IsLockedOutAsync(user)).ReturnsAsync(scenario == "locked");
            users.Setup(manager => manager.CheckPasswordAsync(user, request.Password)).ReturnsAsync(scenario == "success");
            tokens.Setup(manager => manager.GenerateAsync(user.Id, CancellationToken.None)).ReturnsAsync(expected);

            WriteOutput("Arranged input", (nameof(request), request), (nameof(scenario), scenario));

            // Act
            var result = await service.LoginAsync(request);

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.IsSuccess.Should().Be(scenario == "success");
            if (scenario == "success")
                result.Value.Should().BeSameAs(expected);
            else
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            tokens.Verify(manager => manager.GenerateAsync(user.Id, CancellationToken.None),
                scenario == "success" ? Times.Once() : Times.Never());
            unitOfWork.Verify(manager => manager.Commit(CancellationToken.None),
                scenario == "success" ? Times.Once() : Times.Never());
            users.Verify(manager => manager.AccessFailedAsync(user),
                scenario == "wrong-password" ? Times.Once() : Times.Never());
        }

        #endregion

        #region Signup permissions

        [Theory]
        [InlineData("Admin", UserRole.Instructor, true)]
        [InlineData("Admin", UserRole.Student, true)]
        [InlineData("Instructor", UserRole.Student, true)]
        [InlineData("Student", UserRole.Student, false)]
        [InlineData("Instructor", UserRole.Instructor, false)]
        [InlineData("Admin", UserRole.Admin, false)]
        public async Task Signup_AllowsOnlyPermittedRoleAssignments(string callerRole, UserRole assignedRole, bool permitted)
        {
            // Arrange
            var request = Fixture.Build<SignupDTO>().With(dto => dto.Roles, [assignedRole]).Create();
            currentUser.Setup(user => user.GetUserId()).Returns(Fixture.Create<Guid>());
            currentUser.Setup(user => user.IsInRole(It.IsAny<string>())).Returns((string role) => role == callerRole);
            users.Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), request.Password)).ReturnsAsync(IdentityResult.Success);
            roles.Setup(manager => manager.RoleExistsAsync(assignedRole.ToString())).ReturnsAsync(true);
            users.Setup(manager => manager.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);
            unitOfWork.Setup(work => work.ExecuteInTransaction(It.IsAny<Func<CancellationToken, Task<Result>>>(),
                    It.IsAny<Func<Result, bool>>(), CancellationToken.None))
                .Returns((Func<CancellationToken, Task<Result>> operation, Func<Result, bool> _, CancellationToken token) => operation(token));
            Result? result = null;

            WriteOutput("Arranged input", (nameof(request), request), (nameof(callerRole), callerRole));

            // Act
            var exception = await Record.ExceptionAsync(async () => result = await service.SignupAsync(request));

            WriteOutput("Act result", (nameof(result), result), (nameof(exception), exception));

            // Assert
            if (permitted)
            {
                exception.Should().BeNull();
                result!.StatusCode.Should().Be(HttpStatusCode.Created);
                result.IsSuccess.Should().BeTrue();
            }
            else
                exception.Should().BeOfType<ForbiddenException>();

            users.Verify(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), request.Password),
                permitted ? Times.Once() : Times.Never());
            users.Verify(manager => manager.AddToRolesAsync(It.IsAny<ApplicationUser>(),
                It.Is<IEnumerable<string>>(values => values.SequenceEqual(new[] { assignedRole.ToString() }))),
                permitted ? Times.Once() : Times.Never());
        }

        [Fact]
        public async Task Signup_WhenAnonymous_DoesNotCreateAccount()
        {
            // Arrange
            var request = Fixture.Create<SignupDTO>();

            WriteOutput("Arranged input", (nameof(request), request));

            // Act
            var result = await Record.ExceptionAsync(() => service.SignupAsync(request));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeOfType<UnauthenticatedException>();
            users.Verify(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region Refresh

        [Fact]
        public async Task RefreshToken_ForwardsTokenAndCancellation()
        {
            // Arrange
            var token = Fixture.Create<string>();
            var expected = Result<TokensDTO>.Success(Fixture.Create<TokensDTO>());
            using var cancellation = new CancellationTokenSource();
            tokens.Setup(manager => manager.RefreshAsync(token, cancellation.Token)).ReturnsAsync(expected);

            WriteOutput("Arranged input", (nameof(token), token));

            // Act
            var result = await service.RefreshTokenAsync(token, cancellation.Token);

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeSameAs(expected);
            tokens.Verify(manager => manager.RefreshAsync(token, cancellation.Token), Times.Once);
        }

        #endregion
    }
}
