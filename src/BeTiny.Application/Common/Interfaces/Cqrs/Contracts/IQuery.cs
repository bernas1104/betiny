namespace BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

/// <summary>
/// Represents a query that returns a response of the specified type.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IQuery<TResponse> : IRequest<TResponse>;
