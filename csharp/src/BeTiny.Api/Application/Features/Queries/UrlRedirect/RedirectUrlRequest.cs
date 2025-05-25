using BeTiny.Api.Application.Common.Models;
using BeTiny.Api.Domain.Interfaces.CQRS;

namespace BeTiny.Api.Application.Features.Queries.UrlRedirect
{
    public record RedirectUrlRequest(string ShortUrl) : IQuery<Result<RedirectUrlResponse>>;
}
