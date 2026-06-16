namespace BeTiny.Domain.Common.ValueObjects;

/// <summary>
/// Represents the base identifier for aggregate roots, wrapping the underlying identifier value.
/// </summary>
/// <typeparam name="TIdType">The type of the underlying identifier value.</typeparam>
public abstract class AggregateRootId<TIdType> : ValueObject
{
    /// <summary>
    /// Gets the underlying identifier value.
    /// </summary>
    public abstract TIdType Value { get; protected set; }
}
