namespace BeTiny.Application.Features.UrlShortening.Queries.GetByShortCode;

/// <summary>
/// Represents the response for a query by short code.
/// </summary>
/// <param name="OriginalUrl">The original URL associated with the short code.</param>
/// <param name="ExpiresAt">The expiration date and time of the short code, if any.</param>
public sealed record GetByShortCodeResponse(
    string OriginalUrl,
    DateTime? ExpiresAt
);
