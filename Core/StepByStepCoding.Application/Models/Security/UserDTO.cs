namespace StepByStepCoding.Application.Models.Security
{
    public sealed class UserDTO
    {
        public Guid FileId
        {
            get; init;
        }

        public required Guid Id
        {
            get; init;
        }

        public required string FullName
        {
            get; init;
        }

        public required string Email
        {
            get; init;
        }

        public required IReadOnlyList<string> Roles
        {
            get; init;
        }
    }
}
