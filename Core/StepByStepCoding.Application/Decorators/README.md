# Request caching and logging

Queries inherit CacheableRequest<TResponse>; response commands inherit CacheInvalidatingRequest<TResponse>; void commands inherit CacheInvalidatingRequest. These bases implement the appropriate IRequest interface, so concrete requests need only one base. CacheableRequest and CacheInvalidation hold the shared cache metadata without implementing request interfaces. Their old I-prefixed names were removed. Both retain public read-only Prefix properties, private parameterless constructors, and protected constructors accepting a prefix. CacheableRequest stores no request object; virtual Key() serializes the concrete runtime request type, including inherited and derived input properties. Prefix is ignored by JSON and methods are not serialized. Cyclic payload references are ignored.

All academic query declarations inherit CacheableRequest, through CacheableRequest<TResponse>, or CacheablePageRequest<TResponse> for Page/RecordsPerPage. All academic command declarations inherit CacheInvalidatingRequest. Requests use parameterless primary constructors so existing object-initializer and model-binding usage remains compatible.

```csharp
public sealed class GetExamDetailQuery()
    : CacheableRequest<ExamDetailDTO>(CacheGroups.Exams)
{
    public required Guid Id { get; set; }
}

public sealed class DeleteExamCommand()
    : CacheInvalidatingRequest(CacheGroups.Academic)
{
    public required Guid Id { get; set; }
    public required Guid Version { get; set; }
}
```

CacheGroups defines an academic root and nested prefixes such as academic:exams. SQL mutation commands invalidate academic: after success. This deliberately clears dependent views too: enrollment, instructor assignments, publication, grading, and lecture status can affect several features. Live-code state has a separate Redis keyspace and is not removed by academic invalidation.

AcademicCachePolicy authenticates each query before cache lookup and scopes keys by trusted user ID and Admin/Instructor/Student roles. Resource-detail queries recheck current access. Cached list entries are checked for current visibility and entity versions; stale or forbidden rows cause a refresh instead of returning inaccessible data. Cache hits therefore still perform database authorization checks, trading some database work for current access enforcement.

GetCodeSnapshot reads through academic access directly, and GetExamQuestionsList reads current time-sensitive visibility directly. SaveChanges commits each accepted code update to SQL; the global cache decorator invalidates the affected academic and file keys before the hub broadcasts the changes to the file group, including the instructor. It does not cache the update response.

Scrutor registers Logging → Caching → Handler for both response and void handlers. Generic decorator classes are excluded from handler discovery. Logs include request type, elapsed time and outcome without request/response bodies or exception messages. Unmarked non-academic requests pass through without connecting to Redis.

Keys include the prefix, request type, serialized input, and trusted caller scope. Key() can be overridden to choose a custom key, but every result-affecting input must be represented. Raw serialized inputs appear in Redis keys: do not cache requests containing secrets. Property/dictionary ordering is not canonicalized.

Redis expiration uses the existing configured defaults. Invalidation uses CancellationToken.None after success so caller disconnection does not skip cleanup. Redis failures propagate and cannot roll back SQL changes already committed. Prefix scanning is not atomic with concurrent cache fills.

The API now requires ConnectionStrings:Redis for these cacheable academic endpoints and mutation invalidation. Set it through environment variables or secrets as documented by Infrastructure.

Response commands never inherit the void IRequest interface. Scrutor still registers IRequestHandler<ConcreteRequest, Response> or IRequestHandler<ConcreteRequest> exactly as before. Decorators recognize the common metadata bases rather than only the void command variant.

The cache request base classes and CacheGroups live in Application/Utilities/Caching. IRedisService remains in Application/Contracts/Caching.
