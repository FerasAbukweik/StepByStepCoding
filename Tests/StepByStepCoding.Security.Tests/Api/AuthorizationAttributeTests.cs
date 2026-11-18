using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using StepByStepCoding.Controllers;

namespace StepByStepCoding.Security.Tests.Api
{
    public sealed class AuthorizationAttributeTests : TestBase
    {
        public AuthorizationAttributeTests(ITestOutputHelper output) : base(output) { }

        #region Account endpoints

        [Fact]
        public void Signup_RestrictsAccountCreationToStaff()
        {
            // Arrange
            var method = typeof(AuthController).GetMethod(nameof(AuthController.Signup))!;

            WriteOutput("Arranged input", ("Endpoint", method.Name));

            // Act
            var result = method.GetCustomAttribute<AuthorizeAttribute>()?.Roles?.Split(',');

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeEquivalentTo("Admin", "Instructor");
            method.GetCustomAttribute<AllowAnonymousAttribute>().Should().BeNull();
        }

        [Theory]
        [InlineData(nameof(AuthController.Login))]
        [InlineData(nameof(AuthController.RefreshToken))]
        public void LoginAndRefresh_AllowAnonymousAccess(string action)
        {
            // Arrange
            var method = typeof(AuthController).GetMethod(action)!;

            WriteOutput("Arranged input", (nameof(action), action));

            // Act
            var result = method.GetCustomAttribute<AllowAnonymousAttribute>();

            WriteOutput("Act result", ("AllowsAnonymous", result is not null));

            // Assert
            result.Should().NotBeNull();
        }

        #endregion
    }
}
