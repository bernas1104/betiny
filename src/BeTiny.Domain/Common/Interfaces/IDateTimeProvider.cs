namespace BeTiny.Domain.Interfaces;

/// <summary>
/// Provides the current UTC date and time, abstracted for testability.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
