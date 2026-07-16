using System.Linq.Expressions;
using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Common.Models;
using BeTiny.Application.Features.Auth.Commands.Login;
using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BeTiny.UnitTests.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandTest
{
    private readonly IRepository<User, UserId, Guid> _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenProvider _tokenProvider;
    private readonly ILogger<LoginCommand> _logger;
    private readonly LoginCommand _command;

    public LoginCommandTest()
    {
        _repository = Substitute.For<IRepository<User, UserId, Guid>>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenProvider = Substitute.For<ITokenProvider>();
        _logger = Substitute.For<ILogger<LoginCommand>>();

        _command = new LoginCommand(_repository, _passwordHasher, _tokenProvider, _logger);
    }

    [Fact]
    public async Task Handle_WhenCredentialsValid_ReturnsSuccessWithToken()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";

        var now = DateTime.UtcNow;

        var user = User.Create(Email.Create(email), password, _passwordHasher);

        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _repository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(user);

        _tokenProvider.IssueToken(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(new TokenResult("dummy-token", now.AddHours(1)));

        // Act
        var result = await _command.Handle(new LoginRequest(email, password), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().Be("dummy-token");
        result.Value.ExpiresAt.Should().BeCloseTo(now.AddHours(1), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_WhenCredentialsValid_CallsIssueTokenWithUserIdAndEmail()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";

        var user = User.Create(Email.Create(email), password, _passwordHasher);

        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _repository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(user);

        _tokenProvider.IssueToken(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(new TokenResult("dummy-token", DateTime.UtcNow.AddHours(1)));

        // Act
        await _command.Handle(new LoginRequest(email, password), CancellationToken.None);

        // Assert
        _tokenProvider.Received(1).IssueToken(user.Id.Value, user.Email.Value);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsUnauthorizedFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";

        _repository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns((User?)null);

        // Act
        var result = await _command.Handle(new LoginRequest(email, password), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(
                e => e.ErrorType == ErrorTypes.UnauthorizedError
                    && e.PropertyName == "Credentials"
        );
    }

    [Fact]
    public async Task Handle_WhenUserFoundButPasswordWrong_ReturnsUnauthorizedFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";

        var user = User.Create(Email.Create(email), password, _passwordHasher);

        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        _repository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(user);

        // Act
        var result = await _command.Handle(new LoginRequest(email, password), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(
            e => e.ErrorType == ErrorTypes.UnauthorizedError
                && e.PropertyName == "Credentials"
        );
    }

    [Fact]
    public async Task Handle_WhenUserNotFoundAndWrongPasswordProduceEqualErrors()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";

        var user = User.Create(Email.Create(email), password, _passwordHasher);

        _repository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(null, user);

        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        var resultUserNotFound = await _command.Handle(new LoginRequest(email, password), CancellationToken.None);
        var resultWrongPassword = await _command.Handle(new LoginRequest(email, password), CancellationToken.None);

        // Assert
        resultUserNotFound.IsSuccess.Should().BeFalse();
        resultWrongPassword.IsSuccess.Should().BeFalse();
        resultUserNotFound.Errors.Should().BeEquivalentTo(resultWrongPassword.Errors);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_StillCallsVerifyPasswordAgainstDummyHash()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";

        const string dummyHash = "$2a$12$dummyhashvaluethatlooksrealenough";
        _passwordHasher.DummyPasswordHash.Returns(dummyHash);

        _repository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns((User?)null);

        // Act
        await _command.Handle(new LoginRequest(email, password), CancellationToken.None);

        // Assert
        _passwordHasher.Received(1).VerifyPassword(password, _passwordHasher.DummyPasswordHash);
    }

    [Fact]
    public async Task Handle_WhenCredentialsValidButAccountInactive_ReturnsForbiddenFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";

        var user = User.Create(Email.Create(email), password, _passwordHasher);
        user.Deactivate();

        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _repository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(user);

        // Act
        var result = await _command.Handle(new LoginRequest(email, password), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(
            e => e.ErrorType == ErrorTypes.ForbiddenError
                && e.PropertyName == "Account"
        );
    }

    [Fact]
    public async Task Handle_WhenCredentialsValidButAccountSoftDeleted_ReturnsForbiddenFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";

        var user = User.Create(Email.Create(email), password, _passwordHasher);
        user.Delete();

        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        _repository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(user);

        // Act
        var result = await _command.Handle(new LoginRequest(email, password), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(
            e => e.ErrorType == ErrorTypes.ForbiddenError
                && e.PropertyName == "Account"
        );
    }

    [Fact]
    public async Task Handle_WhenEmailInvalidAndValidatorBypassed_ThrowsArgumentException()
    {
        // Arrange
        var email = "invalid-email";
        var password = "Password123!";

        // Act
        Func<Task> act = async () => await _command.Handle(
            new LoginRequest(email, password),
            CancellationToken.None
        );

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenEmailNotNormalized_LooksUpNormalizedEmail()
    {
        // Arrange
        var email = "Test@Example.com";
        var password = "Password123!";
        var normalizedEmail = "test@example.com";

        var user = User.Create(Email.Create(normalizedEmail), password, _passwordHasher);

        _repository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(user);

        // Act
        await _command.Handle(new LoginRequest(email, password), CancellationToken.None);

        // Assert
        await _repository.Received(1).GetByFilterAsync(
            Arg.Is<Expression<Func<User, bool>>>(expr =>
                expr.Compile().Invoke(user) && user.Email.Value == normalizedEmail
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_OnInvalidCredentials_LogsWarningWithRedactedEmail()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var password = "Password123!";

        _repository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns((User?)null);

        // Act
        await _command.Handle(new LoginRequest(email.Value, password), CancellationToken.None);

        // Assert
        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Warning)
            .Where(call => call.GetArguments()[2]!.ToString()!.Contains(email.RedactedValue))
            .Should()
            .ContainSingle();
    }

    [Fact]
    public async Task Handle_OnAccountDisabled_LogsWarningWithRedactedEmail()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";

        var user = User.Create(Email.Create(email), password, _passwordHasher);
        user.Deactivate();

        _repository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(user);

        // Act
        await _command.Handle(new LoginRequest(email, password), CancellationToken.None);

        // Assert
        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Warning)
            .Where(call => call.GetArguments()[2]!.ToString()!.Contains(user.Email.RedactedValue))
            .Should()
            .ContainSingle();
    }
}
