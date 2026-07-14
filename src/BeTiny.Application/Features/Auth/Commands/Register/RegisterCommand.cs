using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Models;
using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Entities;
using BeTiny.Domain.Exceptions;
using BeTiny.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BeTiny.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handles the registration of a new user.
/// </summary>
public class RegisterCommand(
    IRepository<User, UserId, Guid> userRepository,
    IPasswordHasher passwordHasher,
    ILogger<RegisterCommand> logger
) : IRequestHandler<RegisterRequest, Result<RegisterResponse>>
{
    /// <summary>
    /// Handles the registration request.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the registration response or errors.</returns>
    public async Task<Result<RegisterResponse>> Handle(
        RegisterRequest request,
        CancellationToken cancellationToken
    )
    {
        var email = Email.Create(request.Email);

        var existing = await userRepository.GetByFilterAsync(
            u => u.Email == email,
            cancellationToken
        );

        if (existing is not null)
        {
            logger.LogWarning(
                "Registration failed for {Email}. The email is already registered.",
                email.RedactedValue
            );

            return Result<RegisterResponse>.Failure(CreateDuplicateEmailError());
        }

        var user = User.Create(email, request.Password, passwordHasher);

        try
        {
            await userRepository.AddAsync(user, cancellationToken);
        }
        catch (DuplicateEmailException ex)
        {
            userRepository.Detach(user);

            logger.LogWarning(
                ex,
                "Registration failed for {Email} due to a concurrent registration.",
                email.RedactedValue
            );

            return Result<RegisterResponse>.Failure(CreateDuplicateEmailError());
        }

        return Result<RegisterResponse>.Success(
            new RegisterResponse(user.Id.Value, user.Email.Value)
        );
    }

    private static Error CreateDuplicateEmailError()
    {
        return new Error(
            ErrorTypes.ConflictError,
            "Email",
            "The email is already registered.",
            ErrorSeverity.Medium
        );
    }
}
