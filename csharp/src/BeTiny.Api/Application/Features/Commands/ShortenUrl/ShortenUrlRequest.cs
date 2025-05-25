using BeTiny.Api.Application.Common.Models;
using BeTiny.Api.Domain.Interfaces.CQRS;

namespace BeTiny.Api.Application.Features.Commands.ShortenUrl
{
    public record ShortenUrlRequest(string LongUrl) : ICommand<Result<ShortenUrlResponse>>;
}
