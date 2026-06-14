using System.Diagnostics.CodeAnalysis;
using BeTiny.Domain.Common.ValueObjects;

namespace BeTiny.Domain.ValueObjects;

[ExcludeFromCodeCoverage]
public sealed class ShortUrlId : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; }

  private ShortUrlId(Guid value)
  {
    Value = value;
  }

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }

    public static ShortUrlId Create(Guid value)
    {
        return new ShortUrlId(value);
    }

    public static ShortUrlId CreateUnique()
    {
        return new ShortUrlId(Guid.NewGuid());
    }
}
