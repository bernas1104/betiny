using BeTiny.Application.Common.Models;

namespace BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

/// <summary>
/// Defines a handler for a specific type of notification.
/// </summary>
/// <typeparam name="TNotification">The type of notification to handle.</typeparam>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification.
    /// </summary>
    /// <param name="notification">The notification to handle.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Handle(TNotification notification, CancellationToken ct = default);
}
