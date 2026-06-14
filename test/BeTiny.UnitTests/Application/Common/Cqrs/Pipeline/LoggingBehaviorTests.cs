using BeTiny.Application.Common.Cqrs.Pipeline;
using BeTiny.Application.Common.Interfaces.Cqrs.Pipeline;
using Microsoft.Extensions.Logging;

namespace BeTiny.UnitTests.Application.Common.Cqrs.Pipeline;

public sealed class LoggingBehaviorTests
{
    private readonly ILogger<LoggingBehavior<TestRequest, string>> _logger;
    private readonly LoggingBehavior<TestRequest, string> _loggingBehavior;

    public LoggingBehaviorTests()
    {
        _logger = Substitute.For<ILogger<LoggingBehavior<TestRequest, string>>>();
        _loggingBehavior = new LoggingBehavior<TestRequest, string>(_logger);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldLogRequestStartAndEnd()
    {
        var request = new TestRequest { Message = "Test" };
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        
        next.Invoke(Arg.Any<CancellationToken>()).Returns("Response");

        var response = await _loggingBehavior.Handle(request, next);

        response.Should().Be("Response");
        _logger.Received(2)
            .Log(
                Arg.Is<LogLevel>(l => l == LogLevel.Information),
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }
}
