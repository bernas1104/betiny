namespace BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

/// <summary>
/// Defines a handler for a request that produces a response.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IRequestHandler<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request and returns a response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
