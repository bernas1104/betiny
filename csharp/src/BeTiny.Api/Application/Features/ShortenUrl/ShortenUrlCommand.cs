using BeTiny.Api.Domain.Entites;
using BeTiny.Api.Domain.Interfaces;
using BeTiny.Api.Domain.ValueObjects;

namespace BeTiny.Api.Application.Features.ShortenUrl
{
    public class ShortenUrlCommand : ICommandHandler<ShortenUrlRequest, ShortenUrlResponse>
    {
        public Task<ShortenUrlResponse> Handle(
            ShortenUrlRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var url = new Url(
                UrlId.CreateUnique(8742878294792),
                request.LongUrl
            );

            return Task.FromResult(
                new ShortenUrlResponse(
                    $"http://betiny.com/{url.Id}",
                    url.Id.ToString()
                )
            );
        }
    }
}
