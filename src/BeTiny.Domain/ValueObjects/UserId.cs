using BeTiny.Domain.Common.ValueObjects;

namespace BeTiny.Domain.ValueObjects;

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

  public static UserId Create(Guid value)
  {
    return new UserId(value);
  }

  public static UserId CreateUnique()
  {
    return new UserId(Guid.NewGuid());
  }
}
