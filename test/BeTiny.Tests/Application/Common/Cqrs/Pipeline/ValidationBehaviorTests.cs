using BeTiny.Application.Common.Cqrs.Pipeline;
using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Cqrs.Pipeline;
using BeTiny.Application.Common.Models;
using FluentValidation;
using Moq;

namespace BeTiny.Tests.Application.Common.Cqrs.Pipeline;

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
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IValidator<DummyRequest>)))
            .Returns(new DummyRequestValidator());

        var behavior = new ValidationBehavior<DummyRequest, Result<DummyResponse>>(
            serviceProviderMock.Object
        );

        var request = new DummyRequest(string.Empty);
        var called = false;
        RequestHandlerDelegate<Result<DummyResponse>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result<DummyResponse>.Success(new DummyResponse("ok")));
        };

        var result = await behavior.Handle(request, next);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Validation, result.Error);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Validation failed", result.ErrorMessage);
        Assert.False(called);
    }

    [Fact]
    public async Task Handle_ValidRequest_PassesThrough()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IValidator<DummyRequest>)))
            .Returns(new DummyRequestValidator());

        var behavior = new ValidationBehavior<DummyRequest, Result<DummyResponse>>(
            serviceProviderMock.Object
        );

        var request = new DummyRequest("valid");
        var called = false;
        RequestHandlerDelegate<Result<DummyResponse>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result<DummyResponse>.Success(new DummyResponse("ok")));
        };

        var result = await behavior.Handle(request, next);

        Assert.True(result.IsSuccess);
        Assert.True(called);
    }

    [Fact]
    public async Task Handle_NoValidator_PassesThrough()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IValidator<DummyRequest>)))
            .Returns(default(IValidator<DummyRequest>?)!);

        var behavior = new ValidationBehavior<DummyRequest, Result<DummyResponse>>(
            serviceProviderMock.Object
        );

        var request = new DummyRequest(string.Empty);
        var called = false;
        RequestHandlerDelegate<Result<DummyResponse>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result<DummyResponse>.Success(new DummyResponse("ok")));
        };

        var result = await behavior.Handle(request, next);

        Assert.True(result.IsSuccess);
        Assert.True(called);
    }
}
