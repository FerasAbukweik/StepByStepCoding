using StepByStepCoding.Application.Models.Pagination;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Repositories;

namespace StepByStepCoding.EntityFrameworkCore.Repositories
{
    public class Repository<T, TContext> : IRepository<T, TContext>
        where T : class
        where TContext : DbContext
    {
        protected readonly TContext context;

        public Repository(TContext context)
        {
            this.context = context;
        }

        public IQueryable<T> Filter(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, object?>>[]? include = null,
            Expression<Func<T, object?>>? orderBy = null,
            bool orderByDesc = false,
            int? skip = null,
            int? take = null)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            if (skip.HasValue)
                ArgumentOutOfRangeException.ThrowIfNegative(skip.Value);

            if (take.HasValue)
                ArgumentOutOfRangeException.ThrowIfNegative(take.Value);

            IQueryable<T> query = context.Set<T>().AsNoTracking().Where(predicate);

            if (include is not null)
            {
                foreach (var navigation in include)
                    query = query.Include(navigation);
            }

            if (orderBy is not null)
                query = orderByDesc ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);

            if (skip.HasValue)
                query = query.Skip(skip.Value);

            if (take.HasValue)
                query = query.Take(take.Value);

            return query;
        }

        public async Task<PaginatedDTO<T>> GetPageWithCount(IOrderedQueryable<T> query, int page,
            int recordsPerPage, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(recordsPerPage, 1);
            var items = query.AsNoTracking().Skip(checked((page - 1) * recordsPerPage)).Take(recordsPerPage);

            // Keep a row for the count even when the requested page has no items.
            var rows = await items.DefaultIfEmpty()
                .Select(entity => new { Item = entity, TotalCount = query.Count() })
                .AsSingleQuery()
                .ToListAsync(cancellationToken);

            return new PaginatedDTO<T>
            {
                Items = rows.Where(row => row.Item is not null).Select(row => row.Item!).ToArray(),
                TotalCount = rows[0].TotalCount
            };
        }

        public Task<bool> Any(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return context.Set<T>().AsNoTracking().AnyAsync(predicate, cancellationToken);
        }

        public Task<T> Add(T entity)
        {
            context.Add(entity);

            return Task.FromResult(entity);
        }

        public virtual Task Delete(T entity)
        {
            context.Entry(entity).State = EntityState.Deleted;

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await context.Set<T>().AsNoTracking().ToListAsync();
        }

        public Task<T?> GetById(Guid id) => GetById(id, []);

        public Task<T?> GetById(Guid id, Expression<Func<T, object?>>[] include,
            CancellationToken cancellationToken = default)
        {
            return Filter(entity => EF.Property<Guid>(entity, "Id") == id, include)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public Task<int> GetTotalAmountOfRecords()
        {
            return context.Set<T>().CountAsync();
        }

        public virtual Task Update(T entity)
        {
            context.Entry(entity).State = EntityState.Modified;

            return Task.CompletedTask;
        }
    }
}
