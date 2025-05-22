using BeTiny.Api.Domain.Entites;
using BeTiny.Api.Domain.Interfaces.CQRS;
using BeTiny.Api.Domain.Interfaces.Repositories;
using BeTiny.Api.Domain.ValueObjects;

namespace BeTiny.Api.Application.Features.ShortenUrl
{
    public class ShortenUrlCommand : ICommandHandler<ShortenUrlRequest, ShortenUrlResponse>
    {
        private readonly IKVStore _kVStore;

        public ShortenUrlCommand(IKVStore kVStore)
        {
            _kVStore = kVStore;
        }

        public async Task<ShortenUrlResponse> Handle(
            ShortenUrlRequest request,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var counter = await _kVStore.GetAsync<Counter>("Counter");
            if (counter is null)
            {
                throw new Exception("");
            }

            var url = new Url(
                UrlId.CreateUnique(counter.Value),
                request.LongUrl
            );

            counter.Increment();
            await _kVStore.SetAsync("Counter", counter, null, cancellationToken);

            return new ShortenUrlResponse(
                $"http://betiny.com/{url.Id}",
                url.Id.ToString()
            );
        }
    }
}
