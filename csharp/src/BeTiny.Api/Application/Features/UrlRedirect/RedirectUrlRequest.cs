using BeTiny.Api.Domain.Interfaces;

namespace BeTiny.Api.Application.Features.UrlRedirect
{
    public record RedirectUrlRequest(string ShortUrl) : IQuery<RedirectUrlResponse>;
}
