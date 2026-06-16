using System.Diagnostics.CodeAnalysis;
using BeTiny.Domain.Common.ValueObjects;

namespace BeTiny.Domain.ValueObjects;

[ExcludeFromCodeCoverage]
public sealed class UserId : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; }

  private UserId(Guid value)
  {
    Value = value;
  }

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }

  /// <summary>
  /// Creates a new <see cref="UserId"/> from the specified GUID value.
  /// </summary>
  /// <param name="value">The GUID value.</param>
  /// <returns>A new <see cref="UserId"/> instance.</returns>
  public static UserId Create(Guid value)
  {
    return new UserId(value);
  }

  /// <summary>
  /// Creates a new <see cref="UserId"/> with a unique GUID.
  /// </summary>
  /// <returns>A new <see cref="UserId"/> instance.</returns>
  public static UserId CreateUnique()
  {
    return new UserId(Guid.NewGuid());
  }
}
