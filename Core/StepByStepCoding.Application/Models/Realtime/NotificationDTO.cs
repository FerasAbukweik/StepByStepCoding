namespace StepByStepCoding.Application.Models.Realtime
{
    public sealed class NotificationDTO
    {
        public required Guid Id
        {
            get; init;
        }
        public required string Title
        {
            get; init;
        }
        public required string Message
        {
            get; init;
        }
        public required DateTimeOffset CreatedAt
        {
            get; init;
        }
    }
}
