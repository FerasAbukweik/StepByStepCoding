namespace StepByStepCoding.Application.Models.Security
{
    public class RefreshTokenResultDTO
    {
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
