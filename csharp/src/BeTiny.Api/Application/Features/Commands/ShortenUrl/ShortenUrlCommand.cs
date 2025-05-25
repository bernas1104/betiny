using BeTiny.Api.Application.Common.Models;
using BeTiny.Api.Application.Features.Queries.GetUrl;
using BeTiny.Api.Domain.Entites;
using BeTiny.Api.Domain.Interfaces.CQRS;
using BeTiny.Api.Domain.Interfaces.Repositories;
using BeTiny.Api.Domain.ValueObjects;

namespace BeTiny.Api.Application.Features.Commands.ShortenUrl
{
    public class ShortenUrlCommand : ICommandHandler<ShortenUrlRequest, Result<ShortenUrlResponse>>
    {
        private readonly IRepository<Url, UrlId, string> _repository;
        private readonly IQueryHandler<GetUrlRequest, Result<Url>> _query;
        private readonly IKVStore _store;
        private readonly ILogger<ShortenUrlCommand> _logger;

        public ShortenUrlCommand(
            IRepository<Url, UrlId, string> repository,
            IQueryHandler<GetUrlRequest, Result<Url>> query,
            IKVStore store,
            ILogger<ShortenUrlCommand> logger
        )
        {
            _repository = repository;
            _query = query;
            _store = store;
            _logger = logger;
        }

        public async Task<Result<ShortenUrlResponse>> Handle(
            ShortenUrlRequest request,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            Url url;
            var result = await _query.Handle(
                new GetUrlRequest(request.LongUrl),
                cancellationToken
            );

            if (result.IsSuccess)
            {
                url = result.Value!;

                _logger.LogInformation(
                    "[ShortenUrl] Requested URL ({Url}) has already been shortened",
                    request.LongUrl
                );

                return Result<ShortenUrlResponse>.Success(
                        new ShortenUrlResponse(
                        $"http://betiny.com/{url.Id}",
                        url.Id.ToString()
                    )
                );
            }

            _logger.LogInformation(
                "[ShortenUrl] Shortening URL ({Url})",
                request.LongUrl
            );

            var seed = await _store.GetNextHashSeed(cancellationToken);

            url = new Url(
                UrlId.CreateUnique(seed),
                request.LongUrl
            );

            await _repository.AddAsync(url, cancellationToken);

            _logger.LogInformation(
                "[ShortenUrl] Requested URL ({Url}) shortened to ({ShortUrl})",
                request.LongUrl,
                url.Id
            );

            return Result<ShortenUrlResponse>.Success(
                new ShortenUrlResponse(
                    $"http://betiny.com/{url.Id}",
                    url.Id.ToString()
                )
            );
        }
    }
}
