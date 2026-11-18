using StepByStepCoding.Application.Models.Security;

namespace StepByStepCoding.Contracts.Cookie
{
    public interface ICookieService
    {
        void RegisterTokens(TokensDTO tokens);

        string? GetAccessToken();

        string? GetRefreshToken();

        void RemoveTokens();
    }
}
