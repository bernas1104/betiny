using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Domain.Entities;

namespace BeTiny.Application.Events.ClickEvents.Create;

/// <summary>
/// Represents a notification for creating a click event.
/// </summary>
/// <param name="ShortUrl">The short URL associated with the click event.</param>
/// <param name="UserAgent">The user agent of the client that created the click event.</param>
/// <param name="Referer">The referer URL of the click event.</param>
/// <param name="IpAddress">The IP address of the client that created the click event.</param>
public record CreateClickEventNotification(
    ShortUrl ShortUrl,
    string UserAgent,
    string Referer,
    string? IpAddress
) : INotification;
