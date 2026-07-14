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
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
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

    private void LogRequestStart()
    {
        logger.LogInformation(
            "Starting to handle {RequestType}.",
            typeof(TRequest).Name
        );
    }

    private void LogRequestEnd(Stopwatch stopwatch)
    {
        stopwatch.Stop();

        logger.LogInformation(
            "Handled {RequestType} in {ElapsedMilliseconds} ms.",
            typeof(TRequest).Name,
            stopwatch.ElapsedMilliseconds
        );
    }
}
