using BeTiny.Api.Domain.Entites;
using BeTiny.Api.Domain.Interfaces.CQRS;

namespace BeTiny.Api.Application.Features.Commands.CacheShortUrl
{
    public record CacheShortUrlRequest(Url Url) : ICommand;
}
