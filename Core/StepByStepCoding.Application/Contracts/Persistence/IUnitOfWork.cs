namespace StepByStepCoding.Application.Contracts.Persistence
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task<T> ExecuteInTransaction<T>(Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default);

        Task<T> ExecuteInTransaction<T>(Func<CancellationToken, Task<T>> operation,
            Func<T, bool> shouldCommit, CancellationToken cancellationToken = default);

        Task Commit(CancellationToken cancellationToken = default);

        Task Rollback(CancellationToken cancellationToken = default);
    }
}
