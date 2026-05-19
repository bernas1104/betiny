using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;
using Microsoft.AspNetCore.Mvc;

namespace BeTiny.Api.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class UrlShortnerController : ControllerBase
{
    private readonly ISender _sender;

    public UrlShortnerController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost()]
    public async Task<IActionResult> ShortenUrl([FromBody] CreateShortUrlRequest request)
    {
        var result = await _sender.Send(request, new CancellationToken());
        return Ok(result);
    }
}
