using Microsoft.EntityFrameworkCore;
using PaymentReportService.Data;
using System.Linq.Expressions;

namespace PaymentReportService.Repositories;

public sealed class EfRepository<TEntity>(PaymentDbContext dbContext) : IRepository<TEntity> where TEntity : class
{
    public IQueryable<TEntity> Query() => dbContext.Set<TEntity>().AsQueryable();
    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => dbContext.Set<TEntity>().FindAsync([id], cancellationToken).AsTask();
    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => await dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    public void Update(TEntity entity) => dbContext.Set<TEntity>().Update(entity);
    public void Remove(TEntity entity) => dbContext.Set<TEntity>().Remove(entity);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}
