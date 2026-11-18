using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Enums;
using StepByStepCoding.Application.Models.Security;

namespace StepByStepCoding.Controllers
{
    [ApiController]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
    [Route("api/users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly IUserLookupService users;

        public UsersController(IUserLookupService users)
        {
            this.users = users;
        }

        [HttpGet]
        public Task<IReadOnlyList<UserLookupDTO>> Search([FromQuery] UserRole role,
            [FromQuery] string? search, CancellationToken cancellationToken)
            => users.Search(role, search, cancellationToken);
    }
}
