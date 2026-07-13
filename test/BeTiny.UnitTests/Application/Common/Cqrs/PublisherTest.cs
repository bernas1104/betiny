using BeTiny.Application.Common.Cqrs;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

namespace BeTiny.UnitTests.Application.Common.Cqrs;

public sealed class PublisherTest
{
    private readonly ServiceCollection _serviceCollection;
    private readonly ILogger<Publisher> _logger;

    public PublisherTest()
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
    public async Task Publish_WhenHandlerThrows_LogsErrorAndRethrows()
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
        
        Func<Task> act = async () => await publisher.Publish(notification);

        var hasThrown = await act.Should().ThrowAsync<AggregateException>();
        hasThrown.WithInnerException<InvalidOperationException>();

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Error
                && call.GetArguments()[3] is Exception ex && ex == exception)
            .Should().ContainSingle();
    }

    [Fact]
    public async Task Publish_WhenMultipleHandlersAndOneThrows_LogsAndRethrowsAllFaults()
    {
        var exception1 = new InvalidOperationException("Handler 1 failed");
        
        var handler1 = Substitute.For<INotificationHandler<TestNotification>>();
        handler1.Handle(Arg.Any<TestNotification>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception1);

        var handler2 = Substitute.For<INotificationHandler<TestNotification>>();
        handler2.Handle(Arg.Any<TestNotification>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var serviceProvider = _serviceCollection
            .AddSingleton(_logger)
            .AddScoped(_ => handler1)
            .AddScoped(_ => handler2)
            .BuildServiceProvider();
        
        var publisher = new Publisher(serviceProvider, _logger);

        var notification = new TestNotification();
        
        Func<Task> act = async () => await publisher.Publish(notification);

        var hasThrown = await act.Should().ThrowAsync<AggregateException>();
        hasThrown.WithInnerException<InvalidOperationException>()
            .Where(ex => ex.Message == "Handler 1 failed");

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Error
                && call.GetArguments()[3] is Exception ex && ex == exception1)
            .Should().HaveCount(1);
    }
}

public class TestNotification : INotification
{
}
