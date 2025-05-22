
using BeTiny.Api.Domain.Common.Entities;
using BeTiny.Api.Domain.Common.ValueObjects;
using BeTiny.Api.Domain.Interfaces.Repositories;
using BeTiny.Api.Infra.Database.Context;

namespace BeTiny.Api.Infra.Database.Repositories
{
    public class GenericRepository<TEntity, TId, TIdType> : IRepository<TEntity, TId, TIdType>
        where TEntity : AggregateRoot<TId, TIdType>
        where TId : AggregateRootId<TIdType>
    {
        // private readonly BeTinyDbContext _context;

        public GenericRepository()
        {
            // _context = context;
        }

        public Task<TId> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
