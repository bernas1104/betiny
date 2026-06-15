using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Common.Models;
using BeTiny.Application.Events.ClickEvents.Create;
using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BeTiny.Application.Features.UrlShortening.Queries.GetByShortUrl;

/// <summary>
/// Query to get a URL by its short URL.
/// </summary>
public class GetByShortUrlQuery
    : IRequestHandler<GetByShortUrlRequest, Result<GetByShortUrlResponse>>
{
    private readonly IRepository<ShortUrl, ShortUrlId, Guid> _shortUrlRepository;
    private readonly IIpResolver _ipResolver;
    private readonly IDeviceDetector _deviceDetector;
    private readonly IPublisher _publisher;
    private readonly ILogger<GetByShortUrlQuery> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetByShortUrlQuery"/> class.
    /// </summary>
    /// <param name="shortUrlRepository">The repository for URL shortening.</param>
    /// <param name="ipResolver">The service for resolving IP addresses.</param>
    /// <param name="deviceDetector">The service for detecting device types.</param>
    /// <param name="publisher">The publisher for notifications.</param>
    /// <param name="logger">The logger instance.</param>
    public GetByShortUrlQuery(
        IRepository<ShortUrl, ShortUrlId, Guid> shortUrlRepository,
        IIpResolver ipResolver,
        IDeviceDetector deviceDetector,
        IPublisher publisher,
        ILogger<GetByShortUrlQuery> logger
    )
    {
        _shortUrlRepository = shortUrlRepository;
        _ipResolver = ipResolver;
        _deviceDetector = deviceDetector;
        _publisher = publisher;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query to get a URL by its short URL.
    /// </summary>
    /// <param name="request">The request containing the short URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the URL information.</returns>
    public async Task<Result<GetByShortUrlResponse>> Handle(
        GetByShortUrlRequest request,
        CancellationToken cancellationToken
    )
    {
        var shortUrl = await _shortUrlRepository.GetByFilterAsync(
            x => x.ShortCode == request.ShortCode,
            cancellationToken
        );
        shortUrl ??= await _shortUrlRepository.GetByFilterAsync(
            x => x.CustomAlias == request.ShortCode,
            cancellationToken
        );

        if (shortUrl is null || shortUrl.IsExpired())
        {
            return Result<GetByShortUrlResponse>.Failure(CreateError(shortUrl));
        }
        
        var country = await TryGetCountryByIpAsync(
            request.IpAddress,
            cancellationToken
        );

        var clickEvent = CreateClickEvent(shortUrl, request, country);

        await _publisher.Publish(
            new CreateClickEventNotification(clickEvent),
            cancellationToken
        );

        return Result<GetByShortUrlResponse>.Success(
            new GetByShortUrlResponse(
                shortUrl.OriginalUrl,
                shortUrl.ExpiresAt
            )
        );
    }

    private static Error CreateError(ShortUrl? shortUrl)
    {
        if (shortUrl is null)
        {
            return new Error(
                ErrorTypes.NotFoundError,
                null,
                "Short code not found.",
                ErrorSeverity.Medium
            );
        }

        return new Error(
            ErrorTypes.ExpiredError,
            null,
            "Short code has expired.",
            ErrorSeverity.Medium
        );
    }

    private async Task<string> TryGetCountryByIpAsync(
        string? ipAddress,
        CancellationToken ct
    )
    {
        try
        {
            return await _ipResolver.GetCountryByIpAsync(ipAddress, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to resolve country for IP address"
            );
            
            return "Unknown";
        }
    }

    private ClickEvent CreateClickEvent(
        ShortUrl shortUrl,
        GetByShortUrlRequest request,
        string country
    ) => new (
            shortUrl.Id,
            request.IpAddress ?? "Unknown",
            country,
            request.UserAgent ?? "Unknown",
            request.Referer ?? "Unknown",
            _deviceDetector.DetectDeviceType(request.UserAgent)
        );
}
