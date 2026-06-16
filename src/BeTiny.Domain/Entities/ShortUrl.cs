using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using BeTiny.Domain.Common.Entities;
using BeTiny.Domain.Enums;
using BeTiny.Domain.Interfaces;
using BeTiny.Domain.ValueObjects;

namespace BeTiny.Domain.Entities;

public sealed partial class ShortUrl : AggregateRoot<ShortUrlId, Guid>
{
    public UserId? UserId { get; private set; }
    public string OriginalUrl { get; private set; }
    public string AliasUrl { get; private set; }
    public AliasUrlType Type { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public IReadOnlyList<ClickEvent> ClickEvents { get; private set; }

    #pragma warning disable CS8618
    [ExcludeFromCodeCoverage]
    // Private empty constructor needed by EF Core
    private ShortUrl()
    {
    }
    #pragma warning restore

    public ShortUrl(string originalUrl, AliasUrlType type)
    {
        Id = ShortUrlId.CreateUnique();
        OriginalUrl = originalUrl;
        AliasUrl = null!;
        Type = type;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        ClickEvents = [];
    }

    public void SetAliasUrl(string aliasUrl)
    {
        if (string.IsNullOrWhiteSpace(aliasUrl))
            throw new ArgumentException("Shortened URL cannot be null or whitespace.", nameof(aliasUrl));

        if (Type == AliasUrlType.ShortCode && aliasUrl.Length > 7)
            throw new ArgumentException("Short code cannot be longer than 7 characters.", nameof(aliasUrl));

        if (Type == AliasUrlType.CustomAlias && !CustomAliasRegex().IsMatch(aliasUrl))
            throw new ArgumentException(
                "Custom alias must be 3-50 characters and contain only letters, numbers, hyphens, or underscores.",
                nameof(aliasUrl)
            );

        AliasUrl = aliasUrl;
    }

    [GeneratedRegex("^[A-Za-z0-9_-]{3,50}$")]
    private static partial Regex CustomAliasRegex();

    public bool IsExpired(IDateTimeProvider dateTimeProvider)
        => ExpiresAt.HasValue && dateTimeProvider.UtcNow > ExpiresAt.Value;

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
