using BeTiny.Api.Domain.Interfaces.CQRS;
using BeTiny.Api.Domain.Interfaces.Repositories;

namespace BeTiny.Api.Application.Features.Commands.CacheShortUrl
{
    public class CacheShortUrlCommand : ICommandHandler<CacheShortUrlRequest>
    {
        private readonly IKVStore _store;
        private ILogger<CacheShortUrlCommand> _logger;

        public CacheShortUrlCommand(IKVStore store, ILogger<CacheShortUrlCommand> logger)
        {
            _store = store;
            _logger = logger;
        }

        public async Task Handle(
            CacheShortUrlRequest request,
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogInformation(
                "[Redirect Cache] Caching {Url} for future redirect requests",
                request.Url.LongUrl
            );

            await _store.SetAsync(
                request.Url.Id.ToString(),
                request.Url,
                null,
                cancellationToken
            );

            _logger.LogInformation(
                "[Redirect Cache] Url {Url} cached successfully",
                request.Url.LongUrl
            );
        }
    }
}
