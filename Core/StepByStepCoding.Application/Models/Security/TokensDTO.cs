namespace StepByStepCoding.Application.Models.Security
{
    public class TokensDTO
    {
        public required string AccessToken
        {
            get; set;
        }

        public required DateTimeOffset AccessTokenExpiresAt
        {
            get; set;
        }

        public required string RefreshToken
        {
            get; set;
        }

        public required DateTimeOffset RefreshTokenExpiresAt
        {
            get; set;
        }
    }
}
