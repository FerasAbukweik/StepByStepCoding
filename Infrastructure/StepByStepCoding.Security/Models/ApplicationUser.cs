using Microsoft.AspNetCore.Identity;

namespace StepByStepCoding.Security.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;

        public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];
    }
}
