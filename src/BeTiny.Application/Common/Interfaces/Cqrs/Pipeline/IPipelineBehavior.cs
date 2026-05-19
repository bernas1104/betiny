using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

namespace BeTiny.Application.Common.Interfaces.Cqrs.Pipeline;

public interface IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct = default
    );
}

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(
    CancellationToken ct = default
);
