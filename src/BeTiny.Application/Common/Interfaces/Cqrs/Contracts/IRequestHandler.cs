namespace BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

public interface IRequestHandler<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
