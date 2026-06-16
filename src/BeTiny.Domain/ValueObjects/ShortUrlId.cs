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

    /// <summary>
    /// Creates a new <see cref="ShortUrlId"/> from the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value.</param>
    /// <returns>A new <see cref="ShortUrlId"/> instance.</returns>
    public static ShortUrlId Create(Guid value)
    {
        return new ShortUrlId(value);
    }

    /// <summary>
    /// Creates a new <see cref="ShortUrlId"/> with a unique GUID.
    /// </summary>
    /// <returns>A new <see cref="ShortUrlId"/> instance.</returns>
    public static ShortUrlId CreateUnique()
    {
        return new ShortUrlId(Guid.NewGuid());
    }
}
