using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BeTiny.Application.Events.ClickEvents.Create;

/// <summary>
/// Handles the creation of a click event notification.
/// </summary>
public class CreateClickEventNotificationHandler(
    IRepository<ClickEvent, ClickEventId, Guid> repository,
    IIpResolver ipResolver,
    IDeviceDetector deviceDetector,
    ILogger<CreateClickEventNotificationHandler> logger
) : INotificationHandler<CreateClickEventNotification>
{
    /// <summary>
    /// Handles the specified click event notification.
    /// </summary>
    /// <param name="notification">The click event notification to handle.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Handle(
        CreateClickEventNotification notification,
        CancellationToken ct = default
    )
    {
        var country = await ipResolver.GetCountryByIpAsync(notification.IpAddress);
        if (country == "Unknown")
        {
            logger.LogWarning(
                "Could not resolve country of origin for click event of ShortUrl {ShortUrlId}. "
                    + "Defaulting to 'Unknown'.",
                notification.ShortUrl.Id
            );
        }

        var clickEvent = CreateClickEvent(notification.ShortUrl, notification, country);

        try
        {
            await repository.AddAsync(clickEvent, ct);

            logger.LogInformation(
                "Successfully created click event with ID {ClickEventId}.",
                clickEvent.Id
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to create click event for ShortUrl {ShortUrlId}.",
                notification.ShortUrl.Id
            );
            
            throw;
        }
    }

    private ClickEvent CreateClickEvent(
        ShortUrl shortUrl,
        CreateClickEventNotification notification,
        string country
    ) => new (
            shortUrl.Id,
            notification.IpAddress ?? "Unknown",
            country,
            notification.UserAgent,
            notification.Referer,
            deviceDetector.DetectDeviceType(notification.UserAgent)
        );
}
