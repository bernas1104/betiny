using System.Linq.Expressions;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Domain.Common.Entities;
using BeTiny.Domain.Common.ValueObjects;
using BeTiny.Infrastructure.Postgres.Context;
using Microsoft.EntityFrameworkCore;

namespace BeTiny.Infrastructure.Postgres.Repositories;

public class GenericRepository<TEntity, TId, TIdType> : IRepository<TEntity, TId, TIdType>
    where TEntity : AggregateRoot<TId, TIdType>
    where TId : AggregateRootId<TIdType>
{
    private readonly BeTinyContext _context;

    public GenericRepository(BeTinyContext context)
    {
        _context = context;
    }

    public Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _context.Set<TEntity>()
            .Add(entity);

        return Task.CompletedTask;
    }

    public Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity?> GetByFilterAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken ct = default
    )
    {
        ct.ThrowIfCancellationRequested();

        return _context.Set<TEntity>()
            .FirstOrDefaultAsync(filter, ct);
    }

    public Task<IEnumerable<TEntity>> GetPaginatedByFilterAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>> filter,
        CancellationToken ct = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveChanges(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }
}
