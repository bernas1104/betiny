using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using BeTiny.Domain.Common.Entities;
using BeTiny.Domain.Interfaces;
using BeTiny.Domain.ValueObjects;

namespace BeTiny.Domain.Entities;

public sealed partial class ShortUrl : AggregateRoot<ShortUrlId, Guid>
{
    public UserId? UserId { get; private set; }
    public string OriginalUrl { get; private set; }
    public string? ShortCode { get; private set; }
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

    public ShortUrl(string originalUrl)
    {
        Id = ShortUrlId.CreateUnique();
        OriginalUrl = originalUrl;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        ClickEvents = [];
    }

    public void SetShortCode(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
            throw new ArgumentException("Short code cannot be null or whitespace.", nameof(shortCode));

        if (shortCode.Length > 7)
            throw new ArgumentException("Short code cannot be longer than 7 characters.", nameof(shortCode));

        if (CustomAlias != null)
            throw new InvalidOperationException("Custom alias has already been set and cannot be changed.");

        if (ShortCode != null)
            throw new InvalidOperationException("Short code has already been set and cannot be changed.");

        ShortCode = shortCode;
    }

    public void SetCustomAlias(string customAlias)
    {
        if (string.IsNullOrWhiteSpace(customAlias))
            throw new ArgumentException("Custom alias cannot be null or whitespace.", nameof(customAlias));

        if (customAlias.Length < 3 || customAlias.Length > 50)
            throw new ArgumentException("Custom alias must be between 3 and 50 characters long.", nameof(customAlias));

        if (ShortCode != null)
            throw new InvalidOperationException("Short code has already been set and cannot be changed.");

        if (CustomAlias != null)
            throw new InvalidOperationException("Custom alias has already been set and cannot be changed.");

        CustomAlias = customAlias;
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
