using BeTiny.Application.Common.Models;

namespace BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

public interface IRequest<TResponse>;
public interface IRequest : IRequest<Unit>;
