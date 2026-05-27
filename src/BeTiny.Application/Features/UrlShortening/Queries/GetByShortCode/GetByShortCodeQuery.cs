using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Helpers;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Common.Models;
using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BeTiny.Application.Features.UrlShortening.Queries.GetByShortCode;

/// <summary>
/// Query to get a URL by its short code.
/// </summary>
public class GetByShortCodeQuery
    : IRequestHandler<GetByShortCodeRequest, Result<GetByShortCodeResponse>>
{
    private readonly IRepository<ShortUrl, ShortUrlId, Guid> _shortUrlRepository;
    private readonly IRepository<ClickEvent, ClickEventId, Guid> _clickEventRepository;
    private readonly IIpResolver _ipResolver;
    private readonly ILogger<GetByShortCodeQuery> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetByShortCodeQuery"/> class.
    /// </summary>
    /// <param name="shortUrlRepository">The repository for URL shortening.</param>
    /// <param name="clickEventRepository">The repository for click events.</param>
    /// <param name="ipResolver">The service for resolving IP addresses.</param>
    /// <param name="logger">The logger instance.</param>
    public GetByShortCodeQuery(
        IRepository<ShortUrl, ShortUrlId, Guid> shortUrlRepository,
        IRepository<ClickEvent, ClickEventId, Guid> clickEventRepository,
        IIpResolver ipResolver,
        ILogger<GetByShortCodeQuery> logger
    )
    {
        _shortUrlRepository = shortUrlRepository;
        _clickEventRepository = clickEventRepository;
        _ipResolver = ipResolver;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query to get a URL by its short code.
    /// </summary>
    /// <param name="request">The request containing the short code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the URL information.</returns>
    public async Task<Result<GetByShortCodeResponse>> Handle(
        GetByShortCodeRequest request,
        CancellationToken cancellationToken
    )
    {
        var shortUrl = await _shortUrlRepository.GetByFilterAsync(
            x => x.ShortCode.Equals(request.ShortCode),
            cancellationToken
        );

        if (shortUrl is null || shortUrl.IsExpired())
        {
            return Result<GetByShortCodeResponse>.Failure(CreateError(shortUrl));
        }
        
        var country = await TryGetCountryByIpAsync(
            request.IpAddress,
            cancellationToken
        );

        var clickEvent = new ClickEvent(
            shortUrl.Id,
            request.IpAddress ?? "Unknown",
            country,
            request.UserAgent ?? "Unknown",
            request.Referer ?? "Unknown",
            DeviceTypeHelper.DetectDeviceType(request.UserAgent)
        );

        await TrySaveClickEventAsync(clickEvent, cancellationToken);

        return Result<GetByShortCodeResponse>.Success(
            new GetByShortCodeResponse(
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

    private async Task TrySaveClickEventAsync(
        ClickEvent clickEvent,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _clickEventRepository.AddAsync(clickEvent, cancellationToken);
            await _clickEventRepository.SaveChanges(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save click event for ShortUrlId: {ShortUrlId}",
                clickEvent.ShortUrlId
            );
        }
    }
}
