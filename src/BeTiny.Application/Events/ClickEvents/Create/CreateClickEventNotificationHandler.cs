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
        try
        {
            var country = await TryGetCountryByIpAsync(
                notification.IpAddress,
                notification.ShortUrl.Id,
                ct
            );

            var clickEvent = CreateClickEvent(notification.ShortUrl, notification, country);

            await _repository.AddAsync(clickEvent, ct);

            _logger.LogInformation(
                "Successfully created click event with ID {ClickEventId}.",
                clickEvent.Id
            );
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {   
            throw;
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


    // TODO - Method catches ALL exceptions, which is not ideal. Consider implementing more specific 
    // error handling or using a more robust IP resolution service that provides better error information.
    private async Task<string> TryGetCountryByIpAsync(
        string? ipAddress,
        ShortUrlId shortUrlId,
        CancellationToken ct
    )
    {
        try
        {
            return await _ipResolver.GetCountryByIpAsync(ipAddress, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to resolve country for click event of ShortUrl {ShortUrlId}.",
                shortUrlId
            );
            
            return "Unknown";
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
