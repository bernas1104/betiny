using BeTiny.Application.Common.Cqrs;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

namespace BeTiny.UnitTests.Application.Common.Cqrs;

public sealed class PublisherTests
{
    private readonly ServiceCollection _serviceCollection;
    private readonly ILogger<Publisher> _logger;

    public PublisherTests()
    {
        _serviceCollection = new ServiceCollection();
        _logger = Substitute.For<ILogger<Publisher>>();
    }

    [Fact]
    public async Task Publish_WhenNoHandlers_DoesNotThrow()
    {
        var serviceProvider = _serviceCollection
            .AddSingleton(_logger)
            .BuildServiceProvider();
        
        var publisher = new Publisher(serviceProvider, _logger);

        var notification = new TestNotification();
        
        await publisher.Publish(notification);

        _logger.DidNotReceive()
            .Log(
                Arg.Is<LogLevel>(l => l == LogLevel.Information),
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public async Task Publish_WhenWithHandlers_InvokesAllHandlers()
    {
        var handler1 = Substitute.For<INotificationHandler<TestNotification>>();
        var handler2 = Substitute.For<INotificationHandler<TestNotification>>();

        var serviceProvider = _serviceCollection
            .AddSingleton(_logger)
            .AddScoped(_ => handler1)
            .AddScoped(_ => handler2)
            .BuildServiceProvider();
        
        var publisher = new Publisher(serviceProvider, _logger);

        var notification = new TestNotification();
        
        await publisher.Publish(notification);

        await handler1.Received(1).Handle(notification, Arg.Any<CancellationToken>());
        await handler2.Received(1).Handle(notification, Arg.Any<CancellationToken>());

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Information
                && call.GetArguments()[3] is null)
            .Should().ContainSingle();
    }

    [Fact]
    public async Task Publish_WhenHandlerThrows_LogsError()
    {
        var exception = new InvalidOperationException("Handler failed");
        
        var handler = Substitute.For<INotificationHandler<TestNotification>>();
        handler.Handle(Arg.Any<TestNotification>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var serviceProvider = _serviceCollection
            .AddSingleton(_logger)
            .AddScoped(_ => handler)
            .BuildServiceProvider();
        
        var publisher = new Publisher(serviceProvider, _logger);

        var notification = new TestNotification();
        
        await publisher.Publish(notification);

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Error
                && call.GetArguments()[3] is Exception ex && ex == exception)
            .Should().ContainSingle();
    }
}

public class TestNotification : INotification
{
}
