using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

namespace BeTiny.Application.Common.Interfaces.Cqrs
{
    /// <summary>
    /// Defines a contract for sending requests and receiving responses.
    /// </summary>
    public interface ISender
    {
        /// <summary>
        /// Sends a request and returns a response of the specified type.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <returns>The response of the specified type.</returns>
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
    }
}