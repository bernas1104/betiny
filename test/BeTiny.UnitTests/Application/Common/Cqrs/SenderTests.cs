using BeTiny.Application.Common.Cqrs;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.UnitTests.Application.Common.Cqrs;

public sealed class SenderTests
{
    private readonly ServiceCollection _serviceCollection;

    public SenderTests()
    {
        _serviceCollection = new ServiceCollection();
    }

    [Fact]
    public async Task Send_WhenNoBehaviors_ReturnsExpectedResponse()
    {
        var serviceProvider = _serviceCollection
            .AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>()
            .BuildServiceProvider();

        var sender = new Sender(serviceProvider);

        var request = new TestRequest { Message = "Hello, World!" };

        var response = await sender.Send(request);

        request.Message.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Send_WhenWithBehaviors_ReturnsExpectedResponse()
    {
        var serviceProvider = _serviceCollection
            .AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>()
            .AddScoped<IPipelineBehavior<TestRequest, string>, TestPipelineBehavior>()
            .BuildServiceProvider();

        var sender = new Sender(serviceProvider);

        var request = new TestRequest { Message = "Hello, World!" };

        var response = await sender.Send(request);

        response.Should().BeEquivalentTo("Modified: Hello, World!");
    }
}

public sealed class TestRequest : IRequest<string>
{
    public string Message { get; set; } = string.Empty;
}

public sealed class TestRequestHandler : IRequestHandler<TestRequest, string>
{
    public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Message);
    }
}

public sealed class TestPipelineBehavior : IPipelineBehavior<TestRequest, string>
{
    public async Task<string> Handle(
        TestRequest request,
        RequestHandlerDelegate<string> next,
        CancellationToken cancellationToken
    )
    {
        var response = await next();
        return $"Modified: {response}";
    }
}
