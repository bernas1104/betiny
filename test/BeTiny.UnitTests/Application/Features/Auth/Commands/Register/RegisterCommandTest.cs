using System.Linq.Expressions;
using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Features.Auth.Commands.Register;
using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Entities;
using BeTiny.Domain.Exceptions;
using BeTiny.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

namespace BeTiny.UnitTests.Application.Features.Auth.Commands.Register;

public class RegisterCommandTest
{
    private readonly IRepository<User, UserId, Guid> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<RegisterCommand> _logger;
    private readonly RegisterCommand _handler;

    public RegisterCommandTest()
    {
        _userRepository = Substitute.For<IRepository<User, UserId, Guid>>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _logger = Substitute.For<ILogger<RegisterCommand>>();

        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("$2a$12$mockhash");

        _handler = new RegisterCommand(
            _userRepository,
            _passwordHasher,
            _logger
        );
    }

    [Fact]
    public async Task Handle_CreatesUserAndReturnsSuccess_WhenEmailUnique()
    {
        var request = new RegisterRequest("user@example.com", "Password1");

        _userRepository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns((User?)null);

        var result = await _handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().NotBeEmpty();
        result.Value.Email.Should().Be("user@example.com");

        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email.Value == "user@example.com"),
            Arg.Any<CancellationToken>()
        );

        _passwordHasher.Received(1).HashPassword("Password1");
    }

    [Fact]
    public async Task Handle_ReturnsConflictFailure_WhenEmailAlreadyExists()
    {
        var request = new RegisterRequest("user@example.com", "Password1");
        var existing = User.Create(
            Email.Create("user@example.com"),
            "Password1",
            _passwordHasher
        );

        _userRepository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(existing);

        _passwordHasher.ClearReceivedCalls();

        var result = await _handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.ErrorType == ErrorTypes.ConflictError && e.PropertyName == "Email");

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        _passwordHasher.DidNotReceive().HashPassword(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_NormalizesEmailBeforePreCheck()
    {
        var request = new RegisterRequest("User@Example.COM", "Password1");
        Expression<Func<User, bool>>? capturedFilter = null;

        _userRepository.GetByFilterAsync(
            Arg.Do<Expression<Func<User, bool>>>(f => capturedFilter = f),
            Arg.Any<CancellationToken>()
        ).Returns((User?)null);

        await _handler.Handle(request, CancellationToken.None);

        capturedFilter.Should().NotBeNull();

        var user = User.Create(Email.Create("user@example.com"), "Password1", _passwordHasher);
        capturedFilter!.Compile().Invoke(user).Should().BeTrue();

        var differentCase = User.Create(Email.Create("USER@EXAMPLE.COM"), "Password1", _passwordHasher);
        capturedFilter!.Compile().Invoke(differentCase).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsConflictFailure_AndDetaches_WhenAddAsyncThrowsDuplicateEmailException()
    {
        var request = new RegisterRequest("user@example.com", "Password1");

        _userRepository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns((User?)null);

        _userRepository.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DuplicateEmailException("The email is already registered."));

        var result = await _handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.ErrorType == ErrorTypes.ConflictError && e.PropertyName == "Email");

        _userRepository.Received(1).Detach(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_PassesHashedPasswordToRepository()
    {
        var request = new RegisterRequest("user@example.com", "Password1");

        _userRepository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns((User?)null);

        await _handler.Handle(request, CancellationToken.None);

        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.PasswordHash == "$2a$12$mockhash"),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_ThrowsArgumentException_WhenEmailInvalidAndValidatorBypassed()
    {
        var request = new RegisterRequest("notanemail", "Password1");

        _userRepository.GetByFilterAsync(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns((User?)null);

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
