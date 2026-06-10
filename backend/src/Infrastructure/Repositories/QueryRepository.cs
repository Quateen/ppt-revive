using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Domain.Common;
using PPTRevive.Infrastructure.Data;

namespace PPTRevive.Infrastructure.Repositories;
public class QueryRepository<T> : IQueryRepository<T> where T : BaseAuditableEntity
{
    protected readonly ApplicationDbContext _applicationDbContext;
    protected readonly DbSet<T> _dbSet;
    protected IQueryable<T> _query;

    public QueryRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
        _dbSet = _applicationDbContext.Set<T>();
        _query = _dbSet.AsNoTracking();
    }


    public async Task<T?> GetAsync(
        Expression<Func<T, bool>> conditions,
        CancellationToken cancellationToken = default
        )
    {
        return await _query.FirstOrDefaultAsync(conditions, cancellationToken);
    }

    public async Task<TResult?> GetAsync<TResult>(
        Expression<Func<T, bool>> conditions,
        Expression<Func<T, TResult>> columns,
        CancellationToken cancellationToken = default)
    {
        return await _query
            .Where(conditions)
            .Select(columns)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<List<T>> GetAllAsync(
            Expression<Func<T, bool>> conditions,
            int page = 1,
            int count = 1000,
            CancellationToken cancellationToken = default
        )
    {
        int skip = (page - 1) * count;
        return await _query
            .Where(conditions)
            .Skip(skip)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TResult>> GetAllAsync<TResult>(
        string rawSqlQuery,
        CancellationToken cancellationToken = default
        ) where TResult : class
    {
        // Execute the raw SQL query
        var result = await _applicationDbContext.Set<TResult>()
            .FromSqlRaw(rawSqlQuery)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return result;
    }
    public async Task<List<TResult>> GetAllAsync<TResult>(
                            Expression<Func<T, bool>> conditions,
                            Expression<Func<T, TResult>> columns,
                            int page = 1,
                            int count = 1000,
                            CancellationToken cancellationToken = default
        )
    {
        int skip = (page - 1) * count;

        return await _query
            .Where(conditions)
            .Skip(skip)
            .Take(count)
            .Select(columns)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(
            Expression<Func<T, bool>> conditions,
            CancellationToken cancellationToken = default
        )
    {
        return await _query
            .AnyAsync(conditions);
    }

    public async Task<int> CountAsync(
                    Expression<Func<T, bool>> conditions,
                    CancellationToken cancellationToken = default
        )
    {
        return await _query
            .Where(conditions)
            .CountAsync(cancellationToken);
    }

    public async Task<List<T>> GetAllWithIncludesAsync(
    Expression<Func<T, bool>> conditions,
    Func<IQueryable<T>, IQueryable<T>>? include = null,
    int page = 1,
    int count = 1000,
    CancellationToken cancellationToken = default
        )
    {
        int skip = (page - 1) * count;

        IQueryable<T> query = _query.Where(conditions);

        // Apply Include if specified
        if (include != null)
            query = include(query);

        return await query
            .Skip(skip)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    // imported from entry snap

    public async Task<List<T>> GetAll(
    Expression<Func<T, bool>> conditions,
    string? orderBy = null,
    bool ascending = true,
    int page = 1,
    int count = 100,
    CancellationToken cancellationToken = default
)
    {
        var (items, _) = await GetAllInternalAsync<T>(conditions, null, orderBy, ascending, page, count, includeCount: false, cancellationToken);
        return items;
    }

    public async Task<List<TResult>> GetAllResult<TResult>(
        Expression<Func<T, bool>> conditions,
        Expression<Func<T, TResult>> columns,
        string? orderBy = null,
        bool ascending = true,
        int page = 1,
        int count = 100,
        CancellationToken cancellationToken = default
    )
    {
        var (items, _) = await GetAllInternalAsync(conditions, columns, orderBy, ascending, page, count, includeCount: false, cancellationToken);
        return items;
    }

    public async Task<(List<T> Items, int TotalCount)> GetAllWithCountAsync(
        Expression<Func<T, bool>> conditions,
        string? orderBy = null,
        bool ascending = true,
        int page = 1,
        int count = 100,
        CancellationToken cancellationToken = default
    )
    {
        return await GetAllInternalAsync<T>(conditions, null, orderBy, ascending, page, count, includeCount: true, cancellationToken);
    }

    public async Task<(List<TResult> Items, int TotalCount)> GetAllWithCountAsync<TResult>(
        Expression<Func<T, bool>> conditions,
        Expression<Func<T, TResult>> columns,
        string? orderBy = null,
        bool ascending = true,
        int page = 1,
        int count = 100,
        CancellationToken cancellationToken = default
    )
    {
        return await GetAllInternalAsync(conditions, columns, orderBy, ascending, page, count, includeCount: true, cancellationToken);
    }

    public async Task<(List<TResult> Items, int TotalCount)> GetAllInternalAsync<TResult>(
    Expression<Func<T, bool>> conditions,
    Expression<Func<T, TResult>>? columns,
    string? sortBy, // Accept the string representing the property to sort by
    bool ascending,
    int page,
    int count,
    bool includeCount,
    CancellationToken cancellationToken
)
    {
        int skip = (page - 1) * count;

        // Base query with conditions
        var filteredQuery = _query.Where(conditions);

        // Apply dynamic sorting if sortBy is specified
        if (!string.IsNullOrEmpty(sortBy))
        {
            // Split the sortBy into the property names (supporting nested properties)
            var propertyNames = sortBy.Split('.');
            var parameter = Expression.Parameter(typeof(T), "x");

            Expression propertyAccess = parameter;

            // Traverse the property chain (for nested properties like UserProfile.FirstName)
            foreach (var propertyName in propertyNames)
            {
                propertyAccess = Expression.Property(propertyAccess, propertyName); // Access each property
            }

            // Create a lambda expression for sorting
            var lambda = Expression.Lambda(propertyAccess, parameter);

            // Apply sorting (ascending or descending)
            var methodName = ascending ? "OrderBy" : "OrderByDescending";

            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertyAccess.Type);

            var resultExpression = Expression.Call(
                null, // Static method call
                method,
                filteredQuery.Expression,
                lambda
            );

            // Execute the query with the dynamic sorting applied
            filteredQuery = (IQueryable<T>)filteredQuery.Provider.CreateQuery(resultExpression);
        }


        // Get total count if required
        int totalCount = includeCount ? await filteredQuery.CountAsync(cancellationToken) : 0;

        // Apply pagination
        var paginatedQuery = filteredQuery.Skip(skip).Take(count);

        // Handle projection or return full entity
        if (columns != null)
        {
            return (
                Items: await paginatedQuery.Select(columns).ToListAsync(cancellationToken),
                TotalCount: totalCount
            );
        }

        // For full entity case, cast result to TResult
        return (
            Items: (List<TResult>)(object)await paginatedQuery.ToListAsync(cancellationToken),
            TotalCount: totalCount
        );
    }

    public IQueryable<TEntity> SelectFrom<TEntity>() where TEntity : BaseAuditableEntity
    {
        return _applicationDbContext.Set<TEntity>().AsNoTracking();
    }

}
