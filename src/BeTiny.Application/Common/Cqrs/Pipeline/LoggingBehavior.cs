using System.Diagnostics;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Cqrs.Pipeline;
using Microsoft.Extensions.Logging;

namespace BeTiny.Application.Common.Cqrs.Pipeline;

/// <summary>
/// Logs the handling of a request and its response.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the logging of a request and its response.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The response from the next delegate in the pipeline.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct = default
    )
    {
        LogRequestStart();
        var stopwatch = Stopwatch.StartNew();

        var response = await next(ct);

        LogRequestEnd(stopwatch);

        return response;
    }

    public async Task Handle(
        TRequest notification,
        NotificationHandlerDelegate<TResponse> next,
        CancellationToken ct = default
    )
    {
        LogRequestStart();
        var stopwatch = Stopwatch.StartNew();

        await next(ct);

        LogRequestEnd(stopwatch);
    }

    private void LogRequestStart()
    {
        _logger.LogInformation(
            "Starting to handle {RequestType}.",
            typeof(TRequest).Name
        );
    }

    private void LogRequestEnd(Stopwatch stopwatch)
    {
        stopwatch.Stop();

        _logger.LogInformation(
            "Handled {RequestType} in {ElapsedMilliseconds} ms.",
            typeof(TRequest).Name,
            stopwatch.ElapsedMilliseconds
        );
    }
}
