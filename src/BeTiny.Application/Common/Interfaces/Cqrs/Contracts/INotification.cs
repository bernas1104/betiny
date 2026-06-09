namespace BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

/// <summary>
/// Represents a notification that can be handled by an <see cref="INotificationHandler{TNotification}"/>.
/// </summary>
public interface INotification : IRequest<Task>;
