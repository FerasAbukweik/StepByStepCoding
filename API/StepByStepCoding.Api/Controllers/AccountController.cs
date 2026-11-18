using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Contracts.Cookie;

namespace StepByStepCoding.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/auth")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public sealed class AccountController : ControllerBase
    {
        private readonly IAccountService account;
        private readonly ICookieService cookies;

        public AccountController(IAccountService account, ICookieService cookies)
        {
            this.account = account;
            this.cookies = cookies;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDTO>> Me(CancellationToken cancellationToken)
            => Ok(await account.Me(cancellationToken));

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            await account.Logout(cancellationToken);
            cookies.RemoveTokens();

            return NoContent();
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO request, CancellationToken cancellationToken)
        {
            await account.ChangePassword(request, cancellationToken);
            cookies.RemoveTokens();

            return NoContent();
        }
    }
}
