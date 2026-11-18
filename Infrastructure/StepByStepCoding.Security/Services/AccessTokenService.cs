using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Security.Models;
using StepByStepCoding.Security.Options;

namespace StepByStepCoding.Security.Services
{
    public sealed class AccessTokenService : IAccessTokenService
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly IOptions<JwtOptions> _options;

        public AccessTokenService(UserManager<ApplicationUser> users, IOptions<JwtOptions> options)
        {
            this._users = users;
            this._options = options;
        }

        public async Task<AccessTokenResultDTO> GenerateAsync(Guid userId)
        {
            var user = await _users.FindByIdAsync(userId.ToString())
                ?? throw new InvalidOperationException("User does not exist.");

            var now = DateTimeOffset.UtcNow;
            var expires = now.AddMinutes(_options.Value.AccessTokenLifeTime);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? user.Id.ToString())
            };

            foreach (var role in await _users.GetRolesAsync(user))
                claims.Add(new Claim(ClaimTypes.Role, role));

            var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.Key)),
                SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                _options.Value.Issuer,
                _options.Value.Audience,
                claims,
                now.UtcDateTime,
                expires.UtcDateTime,
                creds
                );

            return new AccessTokenResultDTO
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
                AccessTokenExpiresAt = expires
            };
        }
    }
}
