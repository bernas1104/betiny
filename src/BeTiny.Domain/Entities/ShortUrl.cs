using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using BeTiny.Domain.Common.Entities;
using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Enums;
using BeTiny.Domain.Exceptions;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ShortUrl"/> class with the specified original URL and alias type.
    /// </summary>
    /// <param name="originalUrl">The original URL to shorten.</param>
    /// <param name="type">The type of alias (auto-generated short code or custom alias).</param>
    private ShortUrl(string originalUrl, AliasUrlType type)
    {
        Id = ShortUrlId.CreateUnique();
        OriginalUrl = originalUrl;
        AliasUrl = string.Empty;
        Type = type;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        ClickEvents = [];
    }

    /// <summary>
    /// Sets the alias URL for this shortened URL, applying validation rules based on the alias type.
    /// </summary>
    /// <param name="aliasUrl">The alias URL value.</param>
    /// <param name="reservedAliasPolicy">An optional reserved alias policy to check against.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the alias URL is null, whitespace, or does not match type-specific validation rules.
    /// </exception>
    /// <exception cref="ReservedAliasException">
    /// Thrown when the alias URL is reserved and cannot be used.
    /// </exception>
    private void SetAliasUrl(string aliasUrl, IReservedAliasPolicy? reservedAliasPolicy = null)
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

        if (reservedAliasPolicy is not null && reservedAliasPolicy.IsReserved(aliasUrl))
            throw new ReservedAliasException($"The alias '{aliasUrl}' is reserved and cannot be used.");

        AliasUrl = aliasUrl;
    }

    [GeneratedRegex("^[A-Za-z0-9_-]{3,50}$")]
    public static partial Regex CustomAliasRegex();

    /// <summary>
    /// Determines whether this short URL has expired based on the provided date time provider.
    /// </summary>
    /// <param name="dateTimeProvider">The date time provider for obtaining the current UTC time.</param>
    /// <returns><c>true</c> if the URL is expired; otherwise, <c>false</c>.</returns>
    public bool IsExpired(IDateTimeProvider dateTimeProvider)
        => ExpiresAt.HasValue && dateTimeProvider.UtcNow > ExpiresAt.Value;

    /// <summary>
    /// Sets the expiration date and time for this short URL.
    /// </summary>
    /// <param name="expiresAt">The expiration date and time in UTC, or <c>null</c> to clear expiration.</param>
    /// <param name="dateTimeProvider">The date time provider for obtaining the current UTC time.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="expiresAt"/> is not in UTC or is in the past.
    /// </exception>
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

    /// <summary>
    /// Creates a new instance of <see cref="ShortUrl"/> using a custom alias.
    /// </summary>
    /// <param name="originalUrl">The original URL to be shortened.</param>
    /// <param name="shortCode">The short code for the shortened URL.</param>
    /// <param name="expiresAt">The expiration date and time in UTC, or <c>null</c> to clear expiration.</param>
    /// <param name="dateTimeProvider">The date time provider for obtaining the current UTC time.</param>
    /// <param name="reservedAliasPolicy">An optional reserved alias policy to check against.</param>
    /// <returns>A new instance of <see cref="ShortUrl"/> with the specified short code and expiration.</returns>
    public static ShortUrl CreateFromShortCode(
        string originalUrl, 
        string shortCode,
        DateTime? expiresAt,
        IDateTimeProvider dateTimeProvider,
        IReservedAliasPolicy? reservedAliasPolicy = null
    )
    {
        var shortUrl = new ShortUrl(originalUrl, AliasUrlType.ShortCode);
        shortUrl.SetAliasUrl(shortCode, reservedAliasPolicy);
        shortUrl.SetExpiration(expiresAt, dateTimeProvider);
        return shortUrl;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ShortUrl"/> using a custom alias.
    /// </summary>
    /// <param name="originalUrl">The original URL to be shortened.</param>
    /// <param name="customAlias">The custom alias for the shortened URL.</param>
    /// <param name="expiresAt">The expiration date and time in UTC, or <c>null</c> to clear expiration.</param>
    /// <param name="dateTimeProvider">The date time provider for obtaining the current UTC time.</param>
    /// <param name="reservedAliasPolicy">An optional reserved alias policy to check against.</param>
    /// <returns>A new instance of <see cref="ShortUrl"/> with the specified custom alias and expiration.</returns>
    public static ShortUrl CreateFromCustomAlias(
        string originalUrl,
        string customAlias,
        DateTime? expiresAt,
        IDateTimeProvider dateTimeProvider,
        IReservedAliasPolicy? reservedAliasPolicy = null
    )
    {
        var shortUrl = new ShortUrl(originalUrl, AliasUrlType.CustomAlias);
        shortUrl.SetAliasUrl(customAlias, reservedAliasPolicy);
        shortUrl.SetExpiration(expiresAt, dateTimeProvider);
        return shortUrl;
    }
}
