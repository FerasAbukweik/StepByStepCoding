using System.Linq.Expressions;

namespace StepByStepCoding.Application.Contracts.Repositories
{
    public interface IRepository<T, TContext> : IRepository<T>
        where T : class
        where TContext : class
    {
        new Task<T?> GetById(Guid id, Expression<Func<T, object?>>[] include,
            CancellationToken cancellationToken = default);
    }
}
