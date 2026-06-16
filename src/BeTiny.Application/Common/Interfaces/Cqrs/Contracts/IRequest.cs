using BeTiny.Application.Common.Models;

namespace BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

/// <summary>
/// Represents a request that returns a response of the specified type.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IRequest<TResponse>;

/// <summary>
/// Represents a request that returns no response (equivalent to <see cref="IRequest{Unit}"/>).
/// </summary>
public interface IRequest : IRequest<Unit>;
