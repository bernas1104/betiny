using BeTiny.Api.Domain.Interfaces;

namespace BeTiny.Api.Application.Features.UrlRedirect
{
    public class RedirectUrlHandler : IQueryHandler<RedirectUrlRequest, RedirectUrlResponse>
    {
        public Task<RedirectUrlResponse> Handle(
            RedirectUrlRequest request,
            CancellationToken cancellationToken = default
        )
        {
            throw new NotImplementedException();
        }
    }
}
