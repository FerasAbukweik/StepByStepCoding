namespace StepByStepCoding.Application.Models.Security
{
    public sealed class UserLookupDTO
    {
        public required Guid Id { get; init; }

        public required string FullName { get; init; }

        public required string Email { get; init; }
    }
}
