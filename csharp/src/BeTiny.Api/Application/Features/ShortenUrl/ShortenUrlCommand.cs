using BeTiny.Api.Domain.Entites;
using BeTiny.Api.Domain.Interfaces.CQRS;
using BeTiny.Api.Domain.Interfaces.Repositories;
using BeTiny.Api.Domain.ValueObjects;

namespace BeTiny.Api.Application.Features.ShortenUrl
{
    public class ShortenUrlCommand : ICommandHandler<ShortenUrlRequest, ShortenUrlResponse>
    {
        private readonly IRepository<Url, UrlId, string> _repository;
        private readonly IKVStore _kVStore;

        public ShortenUrlCommand(
            IRepository<Url, UrlId, string> repository,
            IKVStore kVStore
        )
        {
            _repository = repository;
            _kVStore = kVStore;
        }

        public async Task<ShortenUrlResponse> Handle(
            ShortenUrlRequest request,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            /* var url = await _repository.GetByFilterAsync(
                u => u.LongUrl.Equals(request.LongUrl),
                cancellationToken
            );
            if (url is not null)
            {
                return new ShortenUrlResponse(
                    $"http://betiny.com/{url.Id}",
                    url.Id.ToString()
                );
            } */

            var seed = await _kVStore.GetNextHashSeed(cancellationToken);

            var url = new Url(
                UrlId.CreateUnique(seed),
                request.LongUrl
            );

            return new ShortenUrlResponse(
                $"http://betiny.com/{url.Id}",
                url.Id.ToString()
            );
        }
    }
}
