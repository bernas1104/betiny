using System.Diagnostics.CodeAnalysis;
using BeTiny.Domain.Common.Entities;
using BeTiny.Domain.Interfaces;
using BeTiny.Domain.ValueObjects;

namespace BeTiny.Domain.Entities;

public sealed class ShortUrl : AggregateRoot<ShortUrlId, Guid>
{
    public UserId? UserId { get; private set; }
    public string OriginalUrl { get; private set; }
    public string ShortCode { get; private set; }
    public string? CustomAlias { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public IReadOnlyList<ClickEvent> ClickEvents { get; private set; }

    #pragma warning disable CS8618
    [ExcludeFromCodeCoverage]
    // Private empty constructor needed by EF Core
    private ShortUrl()
    {
    }
    #pragma warning restore

    public ShortUrl(string originalUrl, string shortCode)
    {
        Id = ShortUrlId.CreateUnique();
        OriginalUrl = originalUrl;
        ShortCode = shortCode;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        ClickEvents = [];
    }

    public bool IsExpired() => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    public void SetExpiration(DateTime? expiresAt, IDateTimeProvider dateTimeProvider)
    {
        if (expiresAt.HasValue)
        {
            if (expiresAt.Value.Kind != DateTimeKind.Utc)
                throw new ArgumentException(
                    "The ExpiresAt must be in UTC.",
                    nameof(expiresAt)
                );

            if (expiresAt.Value <= dateTimeProvider.UtcNow)
                throw new ArgumentException(
                    "The ExpiresAt must be a future date and time.",
                    nameof(expiresAt)
                );

            ExpiresAt = expiresAt;
        }
    }
}
