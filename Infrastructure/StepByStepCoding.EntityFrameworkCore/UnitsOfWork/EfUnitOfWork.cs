using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using StepByStepCoding.Application.Contracts.Persistence;

namespace StepByStepCoding.EntityFrameworkCore.UnitsOfWork
{
    public sealed class EfUnitOfWork<TContext> : IUnitOfWork<TContext>, IDisposable where TContext : DbContext
    {
        private readonly TContext db;
        private IDbContextTransaction? transaction;

        public EfUnitOfWork(TContext db)
        {
            this.db = db;
        }

        private async Task Begin(CancellationToken cancellationToken = default)
        {
            if (transaction is not null)
                throw new InvalidOperationException("A transaction is already active.");

            transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task Commit(CancellationToken cancellationToken = default)
        {
            await db.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
                await DisposeTransaction();
            }
        }

        public async Task Rollback(CancellationToken cancellationToken = default)
        {
            try
            {
                if (transaction is not null)
                    await transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await DisposeTransaction();

                // A database rollback does not reset entities already marked as saved in memory.
                db.ChangeTracker.Clear();
            }
        }

        public Task<T> ExecuteInTransaction<T>(Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            return ExecuteInTransaction(operation, _ => true, cancellationToken);
        }

        public Task<T> ExecuteInTransaction<T>(Func<CancellationToken, Task<T>> operation,
            Func<T, bool> shouldCommit, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentNullException.ThrowIfNull(shouldCommit);

            var strategy = db.Database.CreateExecutionStrategy();

            // A transient database failure must retry the whole transaction.
            return strategy.ExecuteAsync(async token =>
            {
                await Begin(token);

                try
                {
                    var result = await operation(token);

                    if (shouldCommit(result))
                        await Commit(token);
                    else
                        await Rollback(CancellationToken.None);

                    return result;
                }
                catch
                {
                    await Rollback(CancellationToken.None);
                    throw;
                }
            }, cancellationToken);
        }

        public ValueTask DisposeAsync() => DisposeTransaction();

        public void Dispose()
        {
            var activeTransaction = transaction;
            transaction = null;
            activeTransaction?.Dispose();
        }

        private ValueTask DisposeTransaction()
        {
            var activeTransaction = transaction;
            transaction = null;

            return activeTransaction?.DisposeAsync() ?? ValueTask.CompletedTask;
        }
    }
}
