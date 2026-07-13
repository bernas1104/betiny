using System.Linq.Expressions;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Domain.Common.Entities;
using BeTiny.Domain.Common.ValueObjects;
using BeTiny.Domain.Exceptions;
using BeTiny.Infrastructure.Postgres.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BeTiny.Infrastructure.Postgres.Repositories;

/// <summary>
/// Generic repository implementing <see cref="IRepository{TEntity, TId, TIdType}"/> using Entity Framework Core.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TId">The aggregate root ID type.</typeparam>
/// <typeparam name="TIdType">The underlying ID value type.</typeparam>
public class GenericRepository<TEntity, TId, TIdType> : IRepository<TEntity, TId, TIdType>
    where TEntity : AggregateRoot<TId, TIdType>
    where TId : AggregateRootId<TIdType>
{
    private readonly BeTinyContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericRepository{TEntity, TId, TIdType}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public GenericRepository(BeTinyContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            await _context.Set<TEntity>()
                .AddAsync(entity, ct);

            await SaveChanges(ct);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505") // Unique violation
        {
            if (IsUniqueConstraintViolation(pgEx, "IX_ShortUrls_AliasUrl"))
                throw new DuplicateAliasUrlException(
                    $"A shortened URL already exists for the provided value."
                );

            throw;
        }
    }

    private bool IsUniqueConstraintViolation(PostgresException pgEx, string constraintName)
        => pgEx.ConstraintName!.Equals(constraintName, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public Task<TEntity?> GetByFilterAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken ct = default
    )
    {
        ct.ThrowIfCancellationRequested();

        return _context.Set<TEntity>()
            .FirstOrDefaultAsync(filter, ct);
    }

    /// <summary>
    /// Determines whether any entity matching the filter exists in the repository.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns><c>true</c> if any matching entity exists; otherwise, <c>false</c>.</returns>
    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        return _context.Set<TEntity>()
            .AnyAsync(filter, ct);
    }

    /// <inheritdoc/>
    public void Detach(TEntity entity)
    {
        var entry = _context.Entry(entity);
        if (entry.State != EntityState.Detached)
        {
            entry.State = EntityState.Detached;
        }
    }

    private Task<int> SaveChanges(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }
}
