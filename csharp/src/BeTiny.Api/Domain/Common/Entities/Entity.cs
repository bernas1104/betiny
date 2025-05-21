namespace BeTiny.Api.Domain.Common.Entities
{
    public abstract class Entity<TIdType>
    {
        public TIdType Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }

        #pragma warning disable CS8618
        // Empty constructor needed by EF Core
        protected Entity()
        {
        }
        #pragma warning restore

        public Entity(TIdType id)
        {
            Id = id;
            CreatedAt = DateTime.Now;
        }
    }
}
