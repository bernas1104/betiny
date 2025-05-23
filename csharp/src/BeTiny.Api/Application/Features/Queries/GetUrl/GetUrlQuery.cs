using BeTiny.Api.Domain.Entites;

using BeTiny.Api.Domain.Interfaces.CQRS;
using BeTiny.Api.Domain.Interfaces.Repositories;
using BeTiny.Api.Domain.ValueObjects;

namespace BeTiny.Api.Application.Features.Queries.GetUrl
{
    public class GetUrlQuery : IQueryHandler<GetUrlRequest, Url>
    {
        private readonly IRepository<Url, UrlId, string> _repository;
        private readonly ILogger<GetUrlQuery> _logger;

        public GetUrlQuery(
            IRepository<Url, UrlId, string> repository,
            ILogger<GetUrlQuery> logger
        )
        {
            _repository = repository;
            _logger = logger;
        }

        public Task<Url?> Handle(
            GetUrlRequest request,
            CancellationToken cancellationToken = default
        )
        {
            return _repository.GetByFilterAsync(
                l => l.LongUrl == request.LongUrl,
                cancellationToken
            );
        }
    }
}
