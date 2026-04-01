using System.Linq.Expressions;

namespace TaskTracker.Repositories;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetPagedAsync<TOrderKey>(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, TOrderKey>> orderBy,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    ValueTask<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Delete(TEntity entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
