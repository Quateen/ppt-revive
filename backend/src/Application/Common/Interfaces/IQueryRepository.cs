using System.Linq.Expressions;
using PPTRevive.Domain.Common;

namespace PPTRevive.Application.Common.Interfaces;
public interface IQueryRepository<T> where T : BaseAuditableEntity
{
    Task<T?> GetAsync(
                Expression<Func<T, bool>> conditions,
                CancellationToken cancellationToken = default);
    Task<TResult?> GetAsync<TResult>(
            Expression<Func<T, bool>> conditions,
            Expression<Func<T, TResult>> columns,
            CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(
            Expression<Func<T, bool>> conditions,
            int page = 1,
            int count = 1000,
            CancellationToken cancellationToken = default);

    Task<List<TResult>> GetAllAsync<TResult>(
            Expression<Func<T, bool>> conditions,
            Expression<Func<T, TResult>> columns,
            int page = 1,
            int count = 1000,
            CancellationToken cancellationToken = default);

    Task<List<TResult>> GetAllAsync<TResult>(
            string rawSqlQuery,
            CancellationToken cancellationToken = default) where TResult : class;

    Task<bool> AnyAsync(
            Expression<Func<T, bool>> conditions,
            CancellationToken cancellationToken = default);

    Task<int> CountAsync(
            Expression<Func<T, bool>> conditions,
            CancellationToken cancellationToken = default);

    Task<List<T>> GetAllWithIncludesAsync(
            Expression<Func<T, bool>> conditions,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            int page = 1,
            int count = 1000,
            CancellationToken cancellationToken = default);

    IQueryable<TEntity> SelectFrom<TEntity>() where TEntity : BaseAuditableEntity;

    Task<(List<TResult> Items, int TotalCount)> GetAllWithCountAsync<TResult>(
        Expression<Func<T, bool>> conditions,
        Expression<Func<T, TResult>> columns,
        string? orderBy = null,
        bool ascending = true,
        int page = 1,
        int count = 100,
        CancellationToken cancellationToken = default
    );

    Task<(List<T> Items, int TotalCount)> GetAllWithCountAsync(
        Expression<Func<T, bool>> conditions,
        string? orderBy = null,
        bool ascending = true,
        int page = 1,
        int count = 100,
        CancellationToken cancellationToken = default
    );

    Task<List<TResult>> GetAllResult<TResult>(
    Expression<Func<T, bool>> conditions,
    Expression<Func<T, TResult>> columns,
    string? orderBy = null,
    bool ascending = true,
    int page = 1,
    int count = 100,
    CancellationToken cancellationToken = default
);

    Task<List<T>> GetAll(
    Expression<Func<T, bool>> conditions,
    string? orderBy = null,
    bool ascending = true,
    int page = 1,
    int count = 100,
    CancellationToken cancellationToken = default);

}
