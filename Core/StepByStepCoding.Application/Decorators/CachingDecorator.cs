using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Services;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Application.Contracts.Caching;
using StepByStepCoding.Application.Models.Caching;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Decorators
{
    public sealed class CachingDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : class
    {
        private readonly IRequestHandler<TRequest, TResponse> inner;
        private readonly Lazy<IRedisService> redis;
        private readonly Lazy<AcademicCachePolicy> policy;

        public CachingDecorator(IRequestHandler<TRequest, TResponse> inner, Lazy<IRedisService> redis, Lazy<AcademicCachePolicy> policy)
        {
            this.inner = inner;
            this.redis = redis;
            this.policy = policy;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            if (AcademicCachePolicy.Bypass(request))
                return await inner.Handle(request, cancellationToken);

            if (request is CacheInvalidation update)
            {
                var prefix = update.Prefix;
                ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
                // Invalidate only after the handler has persisted the update.
                var result = await inner.Handle(request, cancellationToken);

                await redis.Value.RemoveByPrefix($"{prefix}:", CancellationToken.None);

                if (update.Key() != $"{prefix}:")
                    await redis.Value.RemoveByPrefix(update.Key(), CancellationToken.None);

                return result;
            }

            if (request is not CacheableRequest cachable)
                return await inner.Handle(request, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            var academic = AcademicCachePolicy.AppliesTo(request);
            var scope = academic ? policy.Value.CurrentScope() : null;
            var key = GetCacheKey(cachable, scope);
            var cached = await redis.Value.Get<CachedResponse<TResponse>>(key, cancellationToken);

            if (cached is not null)
            {
                if (!academic || await policy.Value.CanUse(request, cached.Value, cancellationToken))
                    return cached.Value;

                await redis.Value.RemoveByPrefix(cachable.Prefix == CacheGroups.File ? cachable.Key() : $"{cachable.Prefix}:", cancellationToken);
            }

            var response = await inner.Handle(request, cancellationToken);
            await redis.Value.Set(key, new CachedResponse<TResponse>(response), cancellationToken: cancellationToken);

            return response;
        }

        private string GetCacheKey(CacheableRequest cachable, AcademicReadScope? scope)
        {
            var key = cachable.Key();

            // Personalized lists must not share cached results across users.
            return scope is null || cachable.Prefix == CacheGroups.File
                ? key
                : $"{key}:user:{scope.UserId:D}:roles:{scope.IsAdmin}:{scope.IsInstructor}:{scope.IsStudent}";
        }

    }

    public sealed class CachingDecorator<TRequest> : IRequestHandler<TRequest>
        where TRequest : class
    {
        private readonly IRequestHandler<TRequest> inner;
        private readonly Lazy<IRedisService> redis;

        public CachingDecorator(IRequestHandler<TRequest> inner, Lazy<IRedisService> redis)
        {
            this.inner = inner;
            this.redis = redis;
        }

        public async Task Handle(TRequest request, CancellationToken cancellationToken)
        {
            if (request is not CacheInvalidation update)
            {
                await inner.Handle(request, cancellationToken);
                return;
            }

            var prefix = update.Prefix;
            ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
            await inner.Handle(request, cancellationToken);
            await redis.Value.RemoveByPrefix($"{prefix}:", CancellationToken.None);

            if (update.Key() != $"{prefix}:")
                await redis.Value.RemoveByPrefix(update.Key(), CancellationToken.None);
        }
    }
}
