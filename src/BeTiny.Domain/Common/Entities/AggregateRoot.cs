using System.Diagnostics.CodeAnalysis;
using BeTiny.Domain.Common.ValueObjects;

namespace BeTiny.Domain.Common.Entities;

[ExcludeFromCodeCoverage]
public abstract class AggregateRoot<TId, TIdType> : Entity<TId>
    where TId : AggregateRootId<TIdType>
{
    public new TId Id { get; protected set; }

    #pragma warning disable CS8618
    // Empty constructor needed by EF Core
    protected AggregateRoot()
    {
    }
    #pragma warning restore

    protected AggregateRoot(TId id)
    {
        Id = id;
    }
}
