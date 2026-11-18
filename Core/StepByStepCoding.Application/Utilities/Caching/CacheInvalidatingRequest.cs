using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Utilities.Caching
{
    public abstract class CacheInvalidatingRequest(string prefix)
        : CacheInvalidation(prefix), IRequest
    {
    }

    public abstract class CacheInvalidatingRequest<TResponse>(string prefix)
        : CacheInvalidation(prefix), IRequest<TResponse>
    {
    }
}
