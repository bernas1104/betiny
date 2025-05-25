using BeTiny.Api.Application.Common.Models;
using BeTiny.Api.Domain.Entites;

using BeTiny.Api.Domain.Interfaces.CQRS;

namespace BeTiny.Api.Application.Features.Queries.GetUrl
{
    public record GetUrlRequest(string LongUrl) : IQuery<Result<Url>>;
}
