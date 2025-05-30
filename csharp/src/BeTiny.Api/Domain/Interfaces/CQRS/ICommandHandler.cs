namespace BeTiny.Api.Domain.Interfaces.CQRS
{
    public interface ICommandHandler<in TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken = default);
    }

    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        Task Handle(TCommand request, CancellationToken cancellationToken = default);
    }
}
