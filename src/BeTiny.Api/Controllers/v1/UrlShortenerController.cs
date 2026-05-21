using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;
using Microsoft.AspNetCore.Mvc;

namespace BeTiny.Api.Controllers.v1;

/// <summary>
/// Controller for URL shortening operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class UrlShortenerController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlShortenerController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to send commands and queries.</param>
    public UrlShortenerController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Shortens a given URL.
    /// </summary>
    /// <param name="request">The request containing the URL to be shortened.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the URL shortening operation.</returns>
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
