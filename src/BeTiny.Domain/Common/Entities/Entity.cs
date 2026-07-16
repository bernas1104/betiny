using System.Diagnostics.CodeAnalysis;

namespace BeTiny.Domain.Common.Entities;

[ExcludeFromCodeCoverage]
public abstract class Entity<TIdType>
{
    public virtual TIdType Id { get; protected set; }
    public bool IsActive { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }

    #pragma warning disable CS8618
    // Empty constructor needed by EF Core
    protected Entity()
    {
    }
    #pragma warning restore CS8618

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TIdType}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    public Entity(TIdType id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    internal void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
