using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Repositories;
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
    private readonly ILogger<CreateClickEventNotificationHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateClickEventNotificationHandler"/> class.
    /// </summary>
    /// <param name="repository">The repository for click events.</param>
    /// <param name="logger">The logger instance.</param>
    public CreateClickEventNotificationHandler(
        IRepository<ClickEvent, ClickEventId, Guid> repository,
        ILogger<CreateClickEventNotificationHandler> logger
    )
    {
        _repository = repository;
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
        await _repository.AddAsync(notification.ClickEvent, ct);
        await _repository.SaveChanges(ct);
    }
}
