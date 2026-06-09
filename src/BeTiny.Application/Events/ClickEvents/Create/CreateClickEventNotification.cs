using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Domain.Entities;
using FluentValidation;

namespace BeTiny.Application.Events.ClickEvents.Create;

/// <summary>
/// Represents a notification for creating a click event.
/// </summary>
/// <param name="ClickEvent">The click event that was created.</param>
public record CreateClickEventNotification(ClickEvent ClickEvent) : INotification;

public class CreateClickEventNotificationValidator
  : AbstractValidator<CreateClickEventNotification>
{
     /// <summary>
     /// Initializes a new instance of the <see cref="CreateClickEventNotificationValidator"/> class.
     /// </summary>
     public CreateClickEventNotificationValidator()
     {
          RuleFor(x => x.ClickEvent).NotNull();
     }
}
