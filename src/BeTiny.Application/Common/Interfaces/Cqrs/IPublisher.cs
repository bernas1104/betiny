using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

namespace BeTiny.Application.Common.Interfaces.Cqrs;

/// <summary>
/// Defines a contract for publishing notifications.
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Publishes a notification to all registered handlers.
    /// </summary>
    /// <param name="notification">The notification to publish.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Publish<TNotification>(
        TNotification notification,
        CancellationToken ct = default
    ) where TNotification : INotification;
}
