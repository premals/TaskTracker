using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;

namespace TaskTracker.Repositories;

public sealed class GenericRepository<TEntity>(TaskTrackerDbContext dbContext) : IGenericRepository<TEntity>
    where TEntity : class
{
    private readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();

    public Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public Task<List<TEntity>> GetPagedAsync<TOrderKey>(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, TOrderKey>> orderBy,
        CancellationToken cancellationToken = default) =>
        _dbSet.AsNoTracking()
            .OrderBy(orderBy)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _dbSet.CountAsync(cancellationToken);

    public async ValueTask<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default) =>
        await _dbSet.FindAsync([id], cancellationToken);

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        _dbSet.AddAsync(entity, cancellationToken).AsTask();

    public void Update(TEntity entity) => _dbSet.Update(entity);

    public void Delete(TEntity entity) => _dbSet.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
