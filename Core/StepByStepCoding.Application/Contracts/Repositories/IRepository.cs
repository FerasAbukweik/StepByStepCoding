using StepByStepCoding.Application.Models.Pagination;
using System.Linq.Expressions;

namespace StepByStepCoding.Application.Contracts.Repositories
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> Filter(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, object?>>[]? include = null,
            Expression<Func<T, object?>>? orderBy = null,
            bool orderByDesc = false,
            int? skip = null,
            int? take = null);

        Task<PaginatedDTO<T>> GetPageWithCount(IOrderedQueryable<T> query, int page, int recordsPerPage,
            CancellationToken cancellationToken = default);

        Task<bool> Any(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task<T> Add(T entity);

        Task Delete(T entity);

        Task<IEnumerable<T>> GetAll();

        Task<T?> GetById(Guid id);

        Task<T?> GetById(Guid id, Expression<Func<T, object?>>[] include,
            CancellationToken cancellationToken = default);

        Task<int> GetTotalAmountOfRecords();

        Task Update(T entity);
    }
}
