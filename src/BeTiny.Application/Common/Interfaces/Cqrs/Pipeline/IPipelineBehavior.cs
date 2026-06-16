using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

namespace BeTiny.Application.Common.Interfaces.Cqrs.Pipeline;

/// <summary>
/// Defines a middleware component in the request pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request and invokes the next delegate in the pipeline.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct = default
    );
}

/// <summary>
/// Represents the next delegate in the request pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="ct">A cancellation token.</param>
/// <returns>A task representing the asynchronous operation with the response.</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(
    CancellationToken ct = default
);
