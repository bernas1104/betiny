using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BeTiny.Application.Common.Cqrs.Pipeline;

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

            var pipelineBehaviors = scope.ServiceProvider
                .GetServices<IPipelineBehavior<IRequest<Task>, Task>>()
                .ToArray();

            await TryExecuteHandlers(
                notification,
                notificationHandlers,
                pipelineBehaviors, ct
            );
        }
    }

    private async Task TryExecuteHandlers<TNotification>(
        TNotification notification,
        object?[] notificationHandlers,
        IPipelineBehavior<IRequest<Task>, Task>[] pipelineBehaviors,
        CancellationToken ct
    ) where TNotification : INotification
    {
        try
        {
            if (notificationHandlers.Any())
            {
                var tasks = GetPipelineTasks(
                    notification,
                    notificationHandlers,
                    pipelineBehaviors,
                    ct
                );

                await Task.WhenAll(tasks);
            }
        }
        catch (Exception ex)
        {
            _serviceProvider
                .GetRequiredService<ILogger<Publisher>>()
                .LogError(
                    ex,
                    "An error occurred while handling notification of type {NotificationType}.",
                    typeof(TNotification).Name
                );
        }
    }

    private Task[] GetPipelineTasks<TNotification>(
        TNotification notification,
        object?[] notificationHandlers,
        IPipelineBehavior<IRequest<Task>, Task>[] pipelineBehaviors,
        CancellationToken ct
    ) where TNotification : INotification
    {
        if (pipelineBehaviors.Any())
        {
            return notificationHandlers
                .Select(
                    handler =>
                    {
                        var pipeline = pipelineBehaviors
                            .Reverse()
                            .Aggregate(
                                (NotificationHandlerDelegate<Task>)
                                    (ct => ((dynamic)handler!).Handle(notification, ct)),
                                (next, behavior) => ct => behavior.Handle(notification, next, ct)
                             );

                        return pipeline(ct);
                    }
                )
                .ToArray();    
        }
        else
        {
            return notificationHandlers
                .Select(
                    handler => ((INotificationHandler<TNotification>)handler!)
                        .Handle(notification, ct)
                )
                .ToArray();
        }
    }
}
