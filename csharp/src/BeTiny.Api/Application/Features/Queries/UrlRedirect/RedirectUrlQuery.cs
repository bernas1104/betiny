using BeTiny.Api.Application.Features.Commands.CacheShortUrl;
using BeTiny.Api.Domain.Entites;
using BeTiny.Api.Domain.Interfaces.CQRS;
using BeTiny.Api.Domain.Interfaces.Repositories;
using BeTiny.Api.Domain.ValueObjects;

namespace BeTiny.Api.Application.Features.Queries.UrlRedirect
{
    public class RedirectUrlQuery : IQueryHandler<RedirectUrlRequest, RedirectUrlResponse>
    {
        private readonly IRepository<Url, UrlId, string> _repository;
        private readonly IKVStore _store;
        private readonly ICommandHandler<CacheShortUrlRequest> _command;
        private readonly ILogger<RedirectUrlQuery> _logger;

        public RedirectUrlQuery(
            IRepository<Url, UrlId, string> repository,
            IKVStore store,
            ICommandHandler<CacheShortUrlRequest> command,
            ILogger<RedirectUrlQuery> logger
        )
        {
            _repository = repository;
            _store = store;
            _command = command;
            _logger = logger;
        }

        public async Task<RedirectUrlResponse?> Handle(
            RedirectUrlRequest request,
            CancellationToken cancellationToken = default
        )
        {
            _logger.LogInformation(
                "[RedirectUrl] Checking if redirect URL ({ShortUrl}) is cached",
                request.ShortUrl
            );

            var url = await _store.GetAsync<Url>(request.ShortUrl, cancellationToken);
            if (url is not null)
            {
                _logger.LogInformation(
                    "[RedirectUrl] Redirect URL ({ShortUrl}) was found in cached. Redirecting",
                    url.Id
                );

                return new RedirectUrlResponse(url.LongUrl);
            }

            _logger.LogInformation(
                "[RedirectUrl] Checking if URL ({ShortUrl}) is registered",
                request.ShortUrl
            );

            url = await _repository.GetByIdAsync(
                UrlId.Create(request.ShortUrl),
                cancellationToken
            );
            if (url is null)
            {
                _logger.LogError(
                    "[RedirectUrl] URL ({ShortUrl}) not found",
                    request.ShortUrl
                );

                throw new Exception("");
            }

            await _command.Handle(new CacheShortUrlRequest(url), cancellationToken);

            _logger.LogInformation(
                "[RedirectUrl] URL ({ShortUrl}) was found. Redirecting",
                url.Id
            );

            return new RedirectUrlResponse(url.LongUrl);
        }
    }
}
