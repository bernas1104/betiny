using System.Diagnostics.CodeAnalysis;
using BeTiny.Domain.Common.ValueObjects;

namespace BeTiny.Domain.ValueObjects;

[ExcludeFromCodeCoverage]
public sealed class ClickEventId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    private ClickEventId(Guid value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Creates a new <see cref="ClickEventId"/> from the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value.</param>
    /// <returns>A new <see cref="ClickEventId"/> instance.</returns>
    public static ClickEventId Create(Guid value)
    {
        return new ClickEventId(value);
    }

    /// <summary>
    /// Creates a new <see cref="ClickEventId"/> with a unique GUID.
    /// </summary>
    /// <returns>A new <see cref="ClickEventId"/> instance.</returns>
    public static ClickEventId CreateUnique()
    {
        return new ClickEventId(Guid.NewGuid());
    }
}
