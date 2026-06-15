using System.Linq.Expressions;
using BeTiny.Domain.Common.Entities;
using BeTiny.Domain.Common.ValueObjects;

namespace BeTiny.Application.Common.Interfaces.Repositories;

/// <summary>
/// Defines a generic repository interface for managing entities of type 
/// <typeparamref name="TEntity"/> with an identifier of type 
/// <typeparamref name="TId"/> and identifier value type of 
/// <typeparamref name="TIdType"/>.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TId"></typeparam>
/// <typeparam name="TIdType"></typeparam>
public interface IRepository<TEntity, TId, TIdType>
    where TEntity : AggregateRoot<TId, TIdType>
    where TId : AggregateRootId<TIdType>
{
    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(TEntity entity, CancellationToken ct = default);

    /// <summary>
    /// Retrieves an entity based on a filter expression.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> GetByFilterAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken ct = default
    );

    /// <summary>
    /// Checks if any entity exists based on a filter expression.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>True if any entity exists; otherwise, false.</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken ct = default);

    /// <summary>
    /// Saves changes made to the repository. This method should be called after
    /// performing add operations to persist the changes to the data store.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The number of state entries written to the underlying database.</returns>
    Task<int> SaveChanges(CancellationToken ct = default);
}
