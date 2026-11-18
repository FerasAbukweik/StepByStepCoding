# Shared Entity Framework Core infrastructure

Security and Persistence reference this project. It references Application for contracts and EF Core Relational for database operations; it does not reference either owning layer or a specific database provider.

- `Repository<TEntity, TContext>` implements the shared CRUD/filter contract. Reads use `AsNoTracking`; writes are staged for the unit of work.
- `EfUnitOfWork<TContext>` implements commits and retryable transactions.
- `AddEntityFrameworkServices()` registers both open generic implementations once. Each layer calls it after registering its DbContext.

Inject `IRepository<TEntity, TContext>` and `IUnitOfWork<TContext>` to select the owning database. These context-specific Application interfaces inherit the existing provider-independent contracts without taking an EF dependency.

For specialized repositories, inherit the implementation and implement the entity contract:

```csharp
public sealed class ExampleRepository
    : Repository<Example, ApplicationDbContext>, IExampleRepository
{
    public ExampleRepository(ApplicationDbContext context) : base(context)
    {
    }
}
```

The layer owning the entity registers `IExampleRepository` as scoped. Both the repository and `IUnitOfWork<ApplicationDbContext>` receive the same scoped DbContext. Security uses `ApplicationSecurityDbContext` instead, so commits cannot accidentally target the other layer's database.
