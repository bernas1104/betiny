using System.Linq.Expressions;

using BeTiny.Api.Domain.Common.Entities;
using BeTiny.Api.Domain.Common.ValueObjects;
using BeTiny.Api.Domain.Interfaces.Repositories;
using BeTiny.Api.Infra.Database.Context;

using Microsoft.EntityFrameworkCore;

namespace BeTiny.Api.Infra.Database.Repositories
{
    public class GenericRepository<TEntity, TId, TIdType> : IRepository<TEntity, TId, TIdType>
        where TEntity : AggregateRoot<TId, TIdType>
        where TId : AggregateRootId<TIdType>
    {
        private readonly BeTinyDbContext _context;
        private readonly DbSet<TEntity> _entities;

        public GenericRepository(BeTinyDbContext context)
        {
            _context = context;
            _entities = context.Set<TEntity>();
        }

        public async Task<TId> AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default
        )
        {
            await _entities.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        public Task<TEntity?> GetByFilterAsync(
            Expression<Func<TEntity, bool>> filter,
            CancellationToken cancellationToken = default
        )
        {
            return _entities.FirstOrDefaultAsync(filter, cancellationToken);
        }

        public Task<TEntity?> GetByIdAsync(
            TId id,
            CancellationToken cancellationToken = default
        )
        {
            return GetByFilterAsync(
                u => u.Id == id,
                cancellationToken
            );
        }

        public Task<bool> UpdateAsync(
            TEntity entity,
            CancellationToken cancellationToken = default
        )
        {
            throw new NotImplementedException();
        }
    }
}
