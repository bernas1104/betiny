using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Domain.Common.Entities;
using BeTiny.Domain.Common.ValueObjects;
using BeTiny.Infrastructure.Postgres.Context;

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

        return SaveChanges(ct);
    }

    public Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TEntity>> GetPaginatedByFilterAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    private Task<int> SaveChanges(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }
}
