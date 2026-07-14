using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Models;
using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using FluentValidation;

namespace BeTiny.Application.Features.Auth.Commands.Register;

/// <summary>
/// Represents a request to register a new user.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The user's password.</param>
public sealed record RegisterRequest(
    string Email,
    string Password
) : ICommand<Result<RegisterResponse>>;

/// <summary>
/// Validates <see cref="RegisterRequest"/> instances.
/// </summary>
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterRequestValidator"/> class.
    /// </summary>
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .Must(email => (email ?? string.Empty).Trim().Length <= 254)
            .WithMessage("The Email must not exceed 254 characters.")
            .Must(email => Email.EmailRegex().IsMatch((email ?? string.Empty).Trim()))
            .WithMessage("The Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches(User.PasswordRegex())
            .WithMessage(
                "The Password must be between 8 and 72 characters and contain at least one uppercase letter, one lowercase letter, and one digit."
            );
    }
}
