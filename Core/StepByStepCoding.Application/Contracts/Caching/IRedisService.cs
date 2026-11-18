namespace StepByStepCoding.Application.Contracts.Caching
{
    public interface IRedisService
    {
        Task Set<T>(string key, T value, TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);

        Task<T?> Get<T>(string key, CancellationToken cancellationToken = default);

        Task RemoveByPrefix(string prefix, CancellationToken cancellationToken = default);
    }
}
