using BeTiny.Domain.Interfaces;

namespace BeTiny.Infrastructure.Services;

/// <summary>
/// Provides the current UTC date and time by delegating to <see cref="DateTime.UtcNow"/>.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc/>
    public DateTime UtcNow => DateTime.UtcNow;
}
