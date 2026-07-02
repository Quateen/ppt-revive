using Microsoft.EntityFrameworkCore;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Domain.Common;
using PPTRevive.Infrastructure.Data;

namespace PPTRevive.Infrastructure.Repositories;

public class DataRepository<T> : QueryRepository<T>, IDataRepository<T> where T : BaseAuditableEntity
{
    //private readonly ApplicationDbContext _applicationDbContext;

    public DataRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
    {
        _query = _dbSet;
    }

    public void Add(T entity, int addedBy)
    {
        var dateTime = DateTime.UtcNow;

        entity.CreatedBy = addedBy;
        entity.LastModifiedBy = addedBy;
        entity.Created = dateTime;
        entity.LastModified = dateTime;
        _applicationDbContext.Add(entity);

        foreach (var entry in _applicationDbContext.ChangeTracker.Entries())
        {
            //_logger.LogDebug($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
        }
    }

    public void Attach(T entity, int attachedBy)
    {
         var dateTime = DateTime.UtcNow;

        entity.LastModifiedBy = attachedBy;
        entity.LastModified = dateTime;
        _applicationDbContext.Attach(entity);
        _applicationDbContext.Entry(entity).State = EntityState.Modified;
    }

    //public void Remove(T entity, Guid removedBy)
    //{
    //    entity.DeletedBy = removedBy;
    //    entity.DeletedAt = DateTime.UtcNow;
    //}

    public void Delete(T entity)
    {
        _applicationDbContext.Remove(entity);
    }
}

