using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BeTiny.Application.Common.Cqrs;

/// <inheritdoc/>
public class Publisher : IPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Publisher> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Publisher"/> class with
    /// the specified service provider.
    /// </summary> <param name="serviceProvider">The service provider to resolve dependencies.</param>
    /// <param name="logger">The logger to log errors.</param>
    public Publisher(IServiceProvider serviceProvider, ILogger<Publisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
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
        if (notificationHandlers.Any())
        {
            var tasks = notificationHandlers
                .Select(async handler => {
                    await ((dynamic)handler!).Handle((dynamic)notification, ct);
                })
                .ToArray();

            await WhenAll(tasks);

            var faultedTasksExceptions = tasks.Where(t => t.IsFaulted)
                .Select(t => t.Exception?.InnerException)
                .ToArray();

            if (faultedTasksExceptions.Any())
            {
                LogTasksExceptions(faultedTasksExceptions, notification);

                throw new AggregateException(
                    "One or more errors occurred while handling notification of type " +
                        $"{notification.GetType().Name}.",
                    faultedTasksExceptions!
                );
            }

            _logger.LogInformation(
                "Successfully handled notification of type {NotificationType} with {HandlerCount} handlers.",
                notification.GetType().Name,
                notificationHandlers.Length
            );
        }
    }

    private async Task WhenAll(IEnumerable<Task> tasks)
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
            _logger.LogError(
                exception,
                "An error occurred while handling notification of type {NotificationType}.",
                notification.GetType().Name
            );
        }
    }
}
