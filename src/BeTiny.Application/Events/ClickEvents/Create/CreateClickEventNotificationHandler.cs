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
public class CreateClickEventNotificationHandler
    : INotificationHandler<CreateClickEventNotification>
{
    private readonly IRepository<ClickEvent, ClickEventId, Guid> _repository;
    private readonly IIpResolver _ipResolver;
    private readonly IDeviceDetector _deviceDetector;
    private readonly ILogger<CreateClickEventNotificationHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateClickEventNotificationHandler"/> class.
    /// </summary>
    /// <param name="repository">The repository for click events.</param>
    /// <param name="ipResolver">The IP resolver service.</param>
    /// <param name="deviceDetector">The device detector service.</param>
    /// <param name="logger">The logger instance.</param>
    public CreateClickEventNotificationHandler(
        IRepository<ClickEvent, ClickEventId, Guid> repository,
        IIpResolver ipResolver,
        IDeviceDetector deviceDetector,
        ILogger<CreateClickEventNotificationHandler> logger
    )
    {
        _repository = repository;
        _ipResolver = ipResolver;
        _deviceDetector = deviceDetector;
        _logger = logger;
    }

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
        var country = await _ipResolver.GetCountryByIpAsync(notification.IpAddress);
        if (country == "Unknown")
        {
            _logger.LogWarning(
                "Could not resolve country of origin for click event of ShortUrl {ShortUrlId}. "
                    + "Defaulting to 'Unknown'.",
                notification.ShortUrl.Id
            );
        }

        var clickEvent = CreateClickEvent(notification.ShortUrl, notification, country);

        try
        {
            await _repository.AddAsync(clickEvent, ct);

            _logger.LogInformation(
                "Successfully created click event with ID {ClickEventId}.",
                clickEvent.Id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
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
            _deviceDetector.DetectDeviceType(notification.UserAgent)
        );
}
