using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BeTiny.Application.Common.Cqrs;

/// <inheritdoc/>
public class Publisher(IServiceProvider serviceProvider, ILogger<Publisher> logger) : IPublisher
{
    /// <inheritdoc/>
    public async Task Publish<TNotification>(
        TNotification notification,
        CancellationToken ct = default
    ) where TNotification : INotification
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var notificationType = notification.GetType();
        var handlerType = typeof(INotificationHandler<>)
            .MakeGenericType(notificationType);

        var notificationHandlers = scope.ServiceProvider
            .GetServices(handlerType)
            .ToArray();

        await TryExecuteHandlersAsync(notificationHandlers, notification, ct);
    }

    private async Task TryExecuteHandlersAsync(
        object?[] notificationHandlers,
        INotification notification,
        CancellationToken ct
    )
    {
        if (notificationHandlers.Length != 0)
        {
            var tasks = notificationHandlers
                .Select(async handler => {
                    await ((dynamic)handler!).Handle((dynamic)notification, ct);
                })
                .ToArray();

            await WhenAllAsync(tasks);

            var faultedTasksExceptions = tasks.Where(t => t.IsFaulted)
                .Select(t => t.Exception?.InnerException)
                .ToArray();

            if (faultedTasksExceptions.Length != 0)
            {
                LogTasksExceptions(faultedTasksExceptions!, notification);

                throw new AggregateException(
                    "One or more errors occurred while handling notification of type " +
                        $"{notification.GetType().Name}.",
                    faultedTasksExceptions!
                );
            }

            logger.LogInformation(
                "Successfully handled notification of type {NotificationType} with {HandlerCount} handlers.",
                notification.GetType().Name,
                notificationHandlers.Length
            );
        }
    }

    private static async Task WhenAllAsync(IEnumerable<Task> tasks)
    {
        try
        {
            await Task.WhenAll(tasks);
        }
        catch
        {
            // Swallow for later processing of exceptions
        }
    }

    private void LogTasksExceptions(IEnumerable<Exception> exceptions, INotification notification)
    {
        foreach (var exception in exceptions)
        {
            logger.LogError(
                exception,
                "An error occurred while handling notification of type {NotificationType}.",
                notification.GetType().Name
            );
        }
    }
}
