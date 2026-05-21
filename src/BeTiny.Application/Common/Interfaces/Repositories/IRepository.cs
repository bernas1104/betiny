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
    /// Retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);
    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);
    /// <summary>
    /// Deletes an entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(TEntity entity, CancellationToken ct = default);
    /// <summary>
    /// Retrieves a paginated list of entities based on a filter.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="filter">The filter expression.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the 
    /// paginated list of entities.</returns>
    Task<IEnumerable<TEntity>> GetPaginatedByFilterAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>> filter,
        CancellationToken ct = default
    );
    /// <summary>
    /// Saves changes made to the repository. This method should be called after
    /// performing add, update, or delete operations to persist the changes to the data store.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The number of state entries written to the underlying database.</returns>
    Task<int> SaveChanges(CancellationToken ct = default);
}
