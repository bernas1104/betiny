using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Models;
using BeTiny.Application.Events.ClickEvents.Create;
using BeTiny.Domain.Entities;
using BeTiny.Domain.Interfaces;
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
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPublisher _publisher;
    private readonly ILogger<GetByShortUrlQuery> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetByShortUrlQuery"/> class.
    /// </summary>
    /// <param name="shortUrlRepository">The repository for URL shortening.</param>
    /// <param name="dateTimeProvider">The provider for date and time.</param>
    /// <param name="publisher">The publisher for notifications.</param>
    /// <param name="logger">The logger instance.</param>
    public GetByShortUrlQuery(
        IRepository<ShortUrl, ShortUrlId, Guid> shortUrlRepository,
        IDateTimeProvider dateTimeProvider,
        IPublisher publisher,
        ILogger<GetByShortUrlQuery> logger
    )
    {
        _shortUrlRepository = shortUrlRepository;
        _dateTimeProvider = dateTimeProvider;
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
            x => x.AliasUrl == request.ShortUrl,
            cancellationToken
        );

        if (shortUrl is null || shortUrl.IsExpired(_dateTimeProvider))
        {
            _logger.LogWarning(
                "Short URL '{ShortUrl}' not found or expired.",
                request.ShortUrl
            );

            return Result<GetByShortUrlResponse>.Failure(CreateError(shortUrl));
        }

        var notification = new CreateClickEventNotification(
            shortUrl,
            request.UserAgent,
            request.Referer,
            request.IpAddress
        );

        await PublishClickEventAsync(notification, cancellationToken);

        _logger.LogInformation(
            "Short URL '{ShortUrl}' accessed successfully. Redirecting user to original URL.",
            request.ShortUrl
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
                "Short URL not found.",
                ErrorSeverity.Medium
            );
        }

        return new Error(
            ErrorTypes.ExpiredError,
            null,
            "Short URL has expired.",
            ErrorSeverity.Medium
        );
    }

    private async Task PublishClickEventAsync(
        CreateClickEventNotification notification,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _publisher.Publish(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Click event tracking failed for short URL '{ShortUrl}'. Redirect proceeding.",
                notification.ShortUrl.AliasUrl
            );
        }
    }
}
