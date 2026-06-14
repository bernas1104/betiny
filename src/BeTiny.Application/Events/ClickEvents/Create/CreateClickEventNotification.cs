using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Domain.Entities;

namespace BeTiny.Application.Events.ClickEvents.Create;

/// <summary>
/// Represents a notification for creating a click event.
/// </summary>
/// <param name="ClickEvent">The click event that was created.</param>
public record CreateClickEventNotification(ClickEvent ClickEvent) : INotification;
