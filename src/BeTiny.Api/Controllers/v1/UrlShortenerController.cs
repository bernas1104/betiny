using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;
using Microsoft.AspNetCore.Mvc;

namespace BeTiny.Api.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class UrlShortenerController : ControllerBase
{
    private readonly ISender _sender;

    public UrlShortenerController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost()]
    public async Task<IActionResult> ShortenUrl(
        [FromBody] CreateShortUrlRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _sender.Send(request, cancellationToken);
        return Ok(result);
    }
}
