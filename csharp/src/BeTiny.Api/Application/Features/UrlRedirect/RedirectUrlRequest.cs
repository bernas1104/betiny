using BeTiny.Api.Domain.Interfaces.CQRS;

namespace BeTiny.Api.Application.Features.UrlRedirect
{
    public record RedirectUrlRequest(string ShortUrl) : IQuery<RedirectUrlResponse>;
}
