using BeTiny.Api.Domain.Common.ValueObjects;

namespace BeTiny.Api.Domain.Common.Entities
{
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
            CreatedAt = DateTime.UtcNow;
        }
    }
}
