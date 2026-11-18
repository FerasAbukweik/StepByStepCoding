using StepByStepCoding.Application.Enums;
using System.Net;
using StepByStepCoding.Contracts.Cookie;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Application.Common;

namespace StepByStepCoding.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService auth;
        private readonly ICookieService cookies;

        public AuthController(IAuthService auth, ICookieService cookies)
        {
            this.auth = auth;
            this.cookies = cookies;
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupDTO request, CancellationToken cancellationToken)
        {
            var result = await auth.SignupAsync(request, cancellationToken);
            Response.Headers.CacheControl = "no-store";

            // Creating another account must preserve the caller's authentication cookies.
            return result.IsSuccess
                ? StatusCode((int)result.StatusCode)
                : StatusCode((int)result.StatusCode, new ProblemDetails
                {
                    Status = (int)result.StatusCode,
                    Title = result.ErrorMessage
                });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO request, CancellationToken cancellationToken)
            => Respond(await auth.LoginAsync(request, cancellationToken));

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
        {
            var refreshToken = cookies.GetRefreshToken();

            if (string.IsNullOrWhiteSpace(refreshToken))
                return Respond(Result<TokensDTO>.Failure("The refresh token cookie is missing or expired.", HttpStatusCode.Unauthorized));

            return Respond(await auth.RefreshTokenAsync(refreshToken, cancellationToken));
        }

        private IActionResult Respond(Result<TokensDTO> result)
        {
            Response.Headers.CacheControl = "no-store";
            Response.Headers.Pragma = "no-cache";

            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ProblemDetails
                {
                    Status = (int)result.StatusCode,
                    Title = result.ErrorMessage
                });
            }

            // Auth services have committed issuance or rotation before cookies are written.
            cookies.RegisterTokens(result.Value ?? throw new InvalidOperationException("Successful authentication returned no tokens."));

            return StatusCode((int)result.StatusCode);
        }
    }
}
