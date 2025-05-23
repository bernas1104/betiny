using BeTiny.Api.Domain.Interfaces.CQRS;

namespace BeTiny.Api.Application.Features.Commands.ShortenUrl
{
    public record ShortenUrlRequest(string LongUrl) : ICommand<ShortenUrlResponse>;
}
