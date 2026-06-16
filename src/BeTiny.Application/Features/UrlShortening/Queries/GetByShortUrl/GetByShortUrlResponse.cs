namespace BeTiny.Application.Features.UrlShortening.Queries.GetByShortUrl;

/// <summary>
/// Represents the response for a query by short URL.
/// </summary>
/// <param name="OriginalUrl">The original URL associated with the short URL.</param>
/// <param name="ExpiresAt">The expiration date and time of the short URL, if any.</param>
public sealed record GetByShortUrlResponse(
    string OriginalUrl,
    DateTime? ExpiresAt
);
