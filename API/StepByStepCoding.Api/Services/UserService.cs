using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using StepByStepCoding.Application.Contracts.Security;

namespace StepByStepCoding.Services
{
    public sealed class UserService : IUserService
    {
        private readonly IHttpContextAccessor accessor;

        public UserService(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public Guid? GetUserId() => Guid.TryParse(accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier),
            out var id) ? id : null;

        public bool IsInRole(string role) => accessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
}
