using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BeTiny.Application.Common.Cqrs;

/// <inheritdoc/>
public class Publisher : IPublisher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Publisher"/> class with
    /// the specified service provider.
    /// </summary> <param name="serviceProvider">The service provider to resolve dependencies.</param>
    public Publisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public async Task Publish<TNotification>(
        TNotification notification,
        CancellationToken ct = default
    ) where TNotification : INotification
    {
        await using (var scope = _serviceProvider.CreateAsyncScope())
        {
            var notificationType = notification.GetType();
            var handlerType = typeof(INotificationHandler<>)
                .MakeGenericType(notificationType);

            var notificationHandlers = scope.ServiceProvider
                .GetServices(handlerType)
                .ToArray();

            await TryExecuteHandlers(notificationHandlers, notification, ct);
        }
    }

    private async Task TryExecuteHandlers(
        object?[] notificationHandlers,
        INotification notification,
        CancellationToken ct
    )
    {
        try
        {
            if (notificationHandlers.Any())
            {
                var tasks = notificationHandlers
                    .Select(async handler => {
                        await ((dynamic)handler!).Handle((dynamic)notification, ct);
                        return Unit.Value;
                    });

                await Task.WhenAll(tasks);
            }
        }
        catch (Exception ex)
        {
            var logger = _serviceProvider
                .GetRequiredService<ILogger<Publisher>>();

            logger.LogError(
                ex,
                "Failed to publish notification of type {NotificationType}.",
                notification.GetType().Name
            );
        }
    }
}
