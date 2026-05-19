namespace BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;

/// <summary>
/// Represents the response for a create short URL request.
/// </summary>
/// <param name="ShortUrl">The shortened URL.</param>
public sealed record CreateShortUrlResponse(string ShortUrl);
