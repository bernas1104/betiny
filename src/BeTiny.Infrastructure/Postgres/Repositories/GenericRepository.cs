using System.Linq.Expressions;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Domain.Common.Entities;
using BeTiny.Domain.Common.ValueObjects;
using BeTiny.Domain.Exceptions;
using BeTiny.Infrastructure.Postgres.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

    public Task<TEntity?> GetByFilterAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken ct = default
    )
    {
        ct.ThrowIfCancellationRequested();

        return _context.Set<TEntity>()
            .FirstOrDefaultAsync(filter, ct);
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        return _context.Set<TEntity>()
            .AnyAsync(filter, ct);
    }

    private Task<int> SaveChanges(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }
}
