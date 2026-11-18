using Microsoft.Extensions.Options;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Contracts.Cookie;
using StepByStepCoding.Options;

namespace StepByStepCoding.Services
{
    public sealed class CookieService : ICookieService
    {
        private readonly IHttpContextAccessor accessor;
        private readonly CookieKeys keys;

        public CookieService(IHttpContextAccessor accessor, IOptions<CookieKeys> options)
        {
            this.accessor = accessor;
            keys = options.Value;
        }

        public void RegisterTokens(TokensDTO tokens)
        {
            ArgumentNullException.ThrowIfNull(tokens);

            Context.Response.Cookies.Append(keys.AccessToken, tokens.AccessToken,
                TokenOptions(tokens.AccessTokenExpiresAt, AccessTokenPath));
            Context.Response.Cookies.Append(keys.RefreshToken, tokens.RefreshToken,
                TokenOptions(tokens.RefreshTokenExpiresAt, RefreshTokenPath));
        }

        public string? GetAccessToken() => Context.Request.Cookies[keys.AccessToken];

        public string? GetRefreshToken() => Context.Request.Cookies[keys.RefreshToken];

        public void RemoveTokens()
        {
            Context.Response.Cookies.Delete(keys.AccessToken, TokenOptions(null, AccessTokenPath));
            Context.Response.Cookies.Delete(keys.RefreshToken, TokenOptions(null, RefreshTokenPath));
        }

        private HttpContext Context => accessor.HttpContext
            ?? throw new InvalidOperationException("Cookie operations require an active HTTP request.");

        private string AccessTokenPath => Context.Request.PathBase.HasValue ? Context.Request.PathBase.Value! : "/";

        // The browser only sends the refresh token to the endpoint that needs it.
        private string RefreshTokenPath => Context.Request.PathBase.Add("/api/auth/refresh-token").Value!;

        private static CookieOptions TokenOptions(DateTimeOffset? expiresAt, string path)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                Expires = expiresAt,
                Path = path
            };
        }
    }
}
