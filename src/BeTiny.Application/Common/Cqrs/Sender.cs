using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.Application.Common.Cqrs
{
    /// <inheritdoc/>
    public class Sender : ISender
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sender"/> class with
        /// the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
        public Sender(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public async Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken ct = default
        )
        {
            await using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var requestType = request.GetType();
                var handlerType = typeof(IRequestHandler<,>)
                    .MakeGenericType(requestType, typeof(TResponse));

                var requestHandler = scope.ServiceProvider
                    .GetRequiredService(handlerType);

                var behaviorType = typeof(IPipelineBehavior<,>)
                    .MakeGenericType(requestType, typeof(TResponse));

                var pipelineBehaviors = scope.ServiceProvider
                    .GetServices(behaviorType);

                if (pipelineBehaviors.Any())
                {
                    var pipeline = pipelineBehaviors
                        .Reverse()
                        .Aggregate(
                            (RequestHandlerDelegate<TResponse>)
                                (ct => ((dynamic)requestHandler).Handle((dynamic)request, ct)),
                            (next, behavior) => ct => ((dynamic)behavior!).Handle((dynamic)request, next, ct)
                        );

                    return await pipeline(ct);
                }
                else
                    return await ((dynamic)requestHandler).Handle((dynamic)request, ct);
            }
        }
    }
}
