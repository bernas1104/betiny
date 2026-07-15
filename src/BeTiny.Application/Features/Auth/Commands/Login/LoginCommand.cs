using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Common.Models;
using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BeTiny.Application.Features.Auth.Commands.Login;

public class LoginCommand(
    IRepository<User, UserId, Guid> repository,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenGenerator,
    ILogger<LoginCommand> logger
) : IRequestHandler<LoginRequest, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        var user = await repository.GetByFilterAsync(u => u.Email == email, cancellationToken);

        var hashToVerify = user?.PasswordHash ?? passwordHasher.DummyPasswordHash;
        var passwordValid = passwordHasher.VerifyPassword(request.Password, hashToVerify);

        if (user is null || passwordValid is false)
        {
            logger.LogWarning("Invalid login attempt for email: {Email}", email.RedactedValue);
            return Result<LoginResponse>.Failure(CreateInvalidCredentialsError());
        }

        if (user.IsActive is false || user.DeletedAt is not null)
        {
            logger.LogWarning("Inactive or deleted user attempted to log in: {Email}", email.RedactedValue);
            return Result<LoginResponse>.Failure(CreateAccountDisabledError());
        }

        var tokenResult = tokenGenerator.IssueToken(user.Id.Value.ToString(), user.Email.Value);

        return Result<LoginResponse>.Success(
            new LoginResponse(tokenResult.Token, tokenResult.ExpiresAt)
        );
    }

    private static Error CreateInvalidCredentialsError() => new (
        ErrorTypes.UnauthorizedError,
        null,
        "The provided email or password is incorrect.",
        ErrorSeverity.Medium
    );

    private static Error CreateAccountDisabledError() => new (
        ErrorTypes.ForbiddenError,
        null,
        "The account is disabled. Please contact support.",
        ErrorSeverity.Medium
    );
}
