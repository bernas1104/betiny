namespace BeTiny.Application.Features.Auth.Commands.Register;

/// <summary>
/// Represents the response for a successful user registration.
/// </summary>
/// <param name="Id">The unique identifier of the registered user.</param>
/// <param name="Email">The normalized email address of the registered user.</param>
public sealed record RegisterResponse(Guid Id, string Email);
