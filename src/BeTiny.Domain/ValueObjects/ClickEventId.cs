using BeTiny.Domain.Common.ValueObjects;

namespace BeTiny.Domain.ValueObjects;

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

    public static ClickEventId Create(Guid value)
    {
        return new ClickEventId(value);
    }

    public static ClickEventId CreateUnique()
    {
        return new ClickEventId(Guid.NewGuid());
    }
}
