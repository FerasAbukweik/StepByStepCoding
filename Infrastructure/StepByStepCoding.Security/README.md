# Authentication

## Layer ownership

- API owns `Controllers/AuthController.cs`, `Controllers/Auth.http`, HTTP response mapping, middleware, cookie options/contracts, cookie token extraction, and the current-user service.
- Application owns auth/token service contracts, DTOs, results, and shared repository/unit-of-work contracts.
- Security owns Identity users/roles, the refresh-token entity and repository contract/implementation, auth/token implementations, logging decorators, JWT validation, the cleanup worker, and security migrations.
- EntityFrameworkCore owns the reusable `Repository<TEntity, TContext>` and `EfUnitOfWork<TContext>` implementations shared by Security and Persistence.
- Domain contains business-independent base entities; HTTP-status results and authentication persistence are not defined there.

Security does not reference API or Persistence. Application does not reference infrastructure projects. API wires the layers together. The auth controller uses normal API controller discovery.

## Configuration and database

Set `Jwt__Key` through a secret configuration provider to a random signing key of at least 32 UTF-8 bytes. Set `ConnectionStrings__SecurityConnection` to the SQL Server connection string. Configure the issuer, audience, and token lifetimes (in minutes) under `Jwt` in the API appsettings file. Never commit secrets.

```sh
dotnet ef database update --project Infrastructure/StepByStepCoding.Security --startup-project API/StepByStepCoding.Api --context ApplicationSecurityDbContext
```

## Endpoints

- `POST /api/auth/signup`: `{ "fullName": "Test User", "email": "person@example.com", "password": "Password123!" }`
- `POST /api/auth/login`: `{ "email": "person@example.com", "password": "Password123!" }`
- `POST /api/auth/refresh-token`: no body; reads the refresh-token cookie.

Signup returns 201; login and refresh return 200, with empty response bodies. The API calls `ICookieService.RegisterTokens` only after a successful auth result, so issuance/rotation has committed first. Tokens remain internal to the service response and are sent to the browser only through Set-Cookie headers.

Access and refresh cookies are HttpOnly, Secure, host-only, and SameSite=Strict; each expiry matches its token. The access cookie covers the application path; the refresh cookie is restricted to the refresh endpoint. Use HTTPS, including during local development. Cookie names are configured under CookieKeys with access_token/refresh_token defaults.

Browser requests must include credentials (Angular: `withCredentials: true`). This SameSite policy expects the frontend and API to be on the same site, including matching schemes; configure the exact frontend origin in CORS. An explicit Authorization header still takes precedence over the access cookie. Failed auth results do not overwrite cookies. Academic controllers use authorization attributes; auth actions allow anonymous requests. Signup does not grant privileged roles.

## Persistence and cleanup

Refresh tokens use 64 random bytes and are stored as SHA-256 hashes. A conditional delete allows each token to be consumed once. Signup and refresh run through `IUnitOfWork<ApplicationSecurityDbContext>.ExecuteInTransaction`; failed result predicates and exceptions roll back, and transient failures retry the whole transaction. Repositories stage inserts; the owning auth flow commits them.

The expired-token cleanup worker runs on startup and every hour, creates an async scope for each run, and calls `DeleteExpired`. The repository uses `ExecuteDeleteAsync` to immediately delete tokens expired at or before the current UTC time. Cleanup respects cancellation and logs failures without stopping future runs.

## Logging and verification

`AddServices` registers implementations; `AddDecorators` wraps them using Scrutor. Decorator messages begin with `ServiceName.MethodName`, built using `nameof`. Auth outcomes use Information/Warning and errors use Error; token internals use Debug. Logs record outcomes, duration, and exception types without request/results, passwords, tokens, or exception messages. The API middleware handles exception details.

```sh
dotnet test Tests/StepByStepCoding.Security.Tests -m:1
```

MSTest checks auth flows, refresh-token rotation/expiry, SQL Server model configuration, repository behavior, database isolation, decorator logging, API controller discovery, cookie/header precedence, and current-user resolution. Relational behavior tests use SQLite; no live SQL Server is required.
