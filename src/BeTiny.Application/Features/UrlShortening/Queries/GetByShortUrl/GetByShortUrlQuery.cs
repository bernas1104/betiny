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
public class GetByShortUrlQuery(
    IRepository<ShortUrl, ShortUrlId, Guid> shortUrlRepository,
    IDateTimeProvider dateTimeProvider,
    IPublisher publisher,
    ILogger<GetByShortUrlQuery> logger
) : IRequestHandler<GetByShortUrlRequest, Result<GetByShortUrlResponse>>
{
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
        var shortUrl = await shortUrlRepository.GetByFilterAsync(
            x => x.AliasUrl == request.ShortUrl,
            cancellationToken
        );

        if (shortUrl is null || shortUrl.IsExpired(dateTimeProvider))
        {
            logger.LogWarning(
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

        logger.LogInformation(
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
            await publisher.Publish(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Click event tracking failed for short URL '{ShortUrl}'. Redirect proceeding.",
                notification.ShortUrl.AliasUrl
            );
        }
    }
}
