using BeTiny.Application.Common.Cqrs.Pipeline;
using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Cqrs.Pipeline;
using BeTiny.Application.Common.Models;
using FluentValidation;

namespace BeTiny.UnitTests.Application.Common.Cqrs.Pipeline;

public sealed record DummyRequest(string Value) : ICommand<Result<DummyResponse>>;

public sealed record DummyResponse(string Value);

public sealed class DummyRequestValidator : AbstractValidator<DummyRequest>
{
    public DummyRequestValidator()
    {
        RuleFor(x => x.Value).NotEmpty();
    }
}

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_InvalidRequest_ReturnsValidationFailure()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider
            .GetService(typeof(IValidator<DummyRequest>))
            .Returns(new DummyRequestValidator());

        var behavior = new ValidationBehavior<DummyRequest, Result<DummyResponse>>(
            serviceProvider
        );

        var request = new DummyRequest(string.Empty);
        var called = false;
        RequestHandlerDelegate<Result<DummyResponse>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result<DummyResponse>.Success(new DummyResponse("ok")));
        };

        var result = await behavior.Handle(request, next);

        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().AllSatisfy(e => e.ErrorType.Should().Be(ErrorTypes.ValidationError));
        result.Errors.Should().AllSatisfy(e => e.ErrorMessage.Should().Contain("'Value' must not be empty."));
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ValidRequest_PassesThrough()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider
            .GetService(typeof(IValidator<DummyRequest>))
            .Returns(new DummyRequestValidator());

        var behavior = new ValidationBehavior<DummyRequest, Result<DummyResponse>>(
            serviceProvider
        );

        var request = new DummyRequest("valid");
        var called = false;
        RequestHandlerDelegate<Result<DummyResponse>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result<DummyResponse>.Success(new DummyResponse("ok")));
        };

        var result = await behavior.Handle(request, next);

        result.Errors.Should().BeEmpty();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NoValidator_PassesThrough()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider
            .GetService(typeof(IValidator<DummyRequest>))
            .Returns(default(IValidator<DummyRequest>?)!);

        var behavior = new ValidationBehavior<DummyRequest, Result<DummyResponse>>(
            serviceProvider
        );

        var request = new DummyRequest(string.Empty);
        var called = false;
        RequestHandlerDelegate<Result<DummyResponse>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result<DummyResponse>.Success(new DummyResponse("ok")));
        };

        var result = await behavior.Handle(request, next);

        result.Errors.Should().BeEmpty();
        called.Should().BeTrue();
    }
}
