using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Models;
using BeTiny.Domain.ValueObjects;
using FluentValidation;

namespace BeTiny.Application.Features.Auth.Commands.Login;

public sealed record LoginRequest(string Email, string Password) : ICommand<Result<LoginResponse>>;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .Must(email => (email ?? string.Empty).Trim().Length <= 254)
            .WithMessage("Email must not exceed 254 characters.")
            .Must(x => Email.EmailRegex().IsMatch(x?.Trim() ?? string.Empty))
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
