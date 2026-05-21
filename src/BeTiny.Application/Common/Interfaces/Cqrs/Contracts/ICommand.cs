namespace BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

public interface ICommand<TResponse> : IRequest<TResponse>;
