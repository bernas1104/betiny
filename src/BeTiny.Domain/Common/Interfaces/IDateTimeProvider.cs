namespace BeTiny.Domain.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
