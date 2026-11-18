# Redis

`IRedisService` lives in Application; `RedisService` and `RedisOptions` live in Infrastructure. Registration uses `AddOptions(configuration).AddRedis(configuration).AddServices()`.

The three methods are `Set<T>`, `Get<T>`, and `RemoveByPrefix`. Values use JSON serialization. `Set` accepts optional `TimeSpan? absoluteExpiration` and `TimeSpan? slidingExpiration`; omitted values use the `Redis` section of appsettings.json. Defaults are 12 hours absolute and 2 hours sliding. Reads refresh sliding expiration, capped by the absolute expiration. A miss returns `default(T)`; use nullable types when a value-type miss must be distinguishable from zero/false.

`RemoveByPrefix` treats its argument as a literal prefix, escapes Redis glob characters, scans primary nodes in pages, and deletes matching keys only under the configured application `KeyPrefix`. Call it after stopping writes for that lecture. It is not an atomic operation against concurrent writers.

## Configuration

Provide `ConnectionStrings:Redis` through user secrets or the environment variable `ConnectionStrings__Redis`. Do not put credentials in appsettings.json. The Redis connection is shared and reconnects automatically.

When Docker Compose is configured, map values from the local `.env` file into the API container's environment:

```yaml
environment:
  ConnectionStrings__Redis: ${REDIS_CONNECTION_STRING:?Set REDIS_CONNECTION_STRING}
  ConnectionStrings__ApplicationConnection: ${APPLICATION_CONNECTION_STRING:?Set APPLICATION_CONNECTION_STRING}
  ConnectionStrings__SecurityConnection: ${SECURITY_CONNECTION_STRING:?Set SECURITY_CONNECTION_STRING}
  Jwt__Key: ${JWT_KEY:?Set JWT_KEY}
```

Compose's `.env` substitution does not automatically configure ASP.NET Core; the environment mapping is required. `.env` files are ignored by Git and the Docker build context. Configure any future credentials the same way.

File reads use the global caching decorator and the ordinary three-method Redis service. The file query and live edit command share `file:{fileId}`. The edit handler applies changes to the SQL file, commits through the unit of work. The hub broadcasts the accepted deltas after cache invalidation. The decorator only invalidates affected prefixes after a successful update. Query misses load SQL and refill the cache; edit results are never written directly to Redis.
