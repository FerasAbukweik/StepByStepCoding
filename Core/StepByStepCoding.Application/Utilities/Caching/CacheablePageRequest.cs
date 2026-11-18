namespace StepByStepCoding.Application.Utilities.Caching
{
    public abstract class CacheablePageRequest<TResponse>(string prefix) : CacheableRequest<TResponse>(prefix)
    {
        public int Page { get; set; } = 1;

        public int RecordsPerPage { get; set; } = 20;
    }
}
