using BeTiny.Api.Application.Features.Queries.GetUrl;
using BeTiny.Api.Domain.Entites;
using BeTiny.Api.Domain.Interfaces.CQRS;
using BeTiny.Api.Domain.Interfaces.Repositories;
using BeTiny.Api.Domain.ValueObjects;

namespace BeTiny.Api.Application.Features.Commands.ShortenUrl
{
    public class ShortenUrlCommand : ICommandHandler<ShortenUrlRequest, ShortenUrlResponse>
    {
        private readonly IRepository<Url, UrlId, string> _repository;
        private readonly IQueryHandler<GetUrlRequest, Url> _query;
        private readonly IKVStore _store;
        private readonly ILogger<ShortenUrlCommand> _logger;

        public ShortenUrlCommand(
            IRepository<Url, UrlId, string> repository,
            IQueryHandler<GetUrlRequest, Url> query,
            IKVStore store,
            ILogger<ShortenUrlCommand> logger
        )
        {
            _repository = repository;
            _query = query;
            _store = store;
            _logger = logger;
        }

        public async Task<ShortenUrlResponse> Handle(
            ShortenUrlRequest request,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var url = await _query.Handle(new GetUrlRequest(request.LongUrl), cancellationToken);
            if (url is not null)
            {
                return new ShortenUrlResponse(
                    $"http://betiny.com/{url.Id}",
                    url.Id.ToString()
                );
            }

            var seed = await _store.GetNextHashSeed(cancellationToken);

            url = new Url(
                UrlId.CreateUnique(seed),
                request.LongUrl
            );

            await _repository.AddAsync(url, cancellationToken);

            return new ShortenUrlResponse(
                $"http://betiny.com/{url.Id}",
                url.Id.ToString()
            );
        }
    }
}
