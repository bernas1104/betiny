namespace BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

/// <summary>
/// Represents a command that returns a response of the specified type.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse>;
