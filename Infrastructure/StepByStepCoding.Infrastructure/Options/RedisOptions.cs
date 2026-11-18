namespace StepByStepCoding.Infrastructure.Options
{
    public sealed class RedisOptions
    {
        public const string SectionName = "Redis";

        public TimeSpan DefaultAbsoluteExpiration { get; set; } = TimeSpan.FromHours(12);

        public TimeSpan DefaultSlidingExpiration { get; set; } = TimeSpan.FromHours(2);

        public string KeyPrefix { get; set; } = "StepByStepCoding:";
    }
}
