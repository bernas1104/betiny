using System.Diagnostics.CodeAnalysis;
using BeTiny.Domain.Common.ValueObjects;

namespace BeTiny.Domain.Common.Entities;

[ExcludeFromCodeCoverage]
public abstract class AggregateRoot<TId, TIdType> : Entity<TId>
    where TId : AggregateRootId<TIdType>
{
    public override TId Id { get; protected set; }

    #pragma warning disable CS8618
    // Empty constructor needed by EF Core
    protected AggregateRoot()
    {
    }
    #pragma warning restore

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId, TIdType}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The aggregate root identifier.</param>
    protected AggregateRoot(TId id)
    {
        Id = id;
    }
}
