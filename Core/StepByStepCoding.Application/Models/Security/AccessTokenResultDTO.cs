namespace StepByStepCoding.Application.Models.Security
{
    public class AccessTokenResultDTO
    {
        public required string AccessToken
        {
            get; set;
        }

        public required DateTimeOffset AccessTokenExpiresAt
        {
            get; set;
        }
    }
}
