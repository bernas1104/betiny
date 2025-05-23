using System.Linq.Expressions;

using BeTiny.Api.Domain.Common.Entities;
using BeTiny.Api.Domain.Common.ValueObjects;

namespace BeTiny.Api.Domain.Interfaces.Repositories
{
    public interface IRepository<TEntity, TId, TIdType>
        where TEntity : AggregateRoot<TId, TIdType>
        where TId : AggregateRootId<TIdType>
    {
        Task<TId> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<TEntity?> GetByFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
        Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}
