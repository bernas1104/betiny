using System.Diagnostics.CodeAnalysis;
using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;
using BeTiny.Application.Features.UrlShortening.Queries.GetByShortUrl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace BeTiny.Api.Controllers.v1;

/// <summary>
/// Controller for URL shortening operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[ExcludeFromCodeCoverage]
public class UrlShortenerController(ISender sender, ILogger<UrlShortenerController> logger)
    : Controller(logger)
{
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
        var result = await sender.Send(request, cancellationToken);
        return HandleResult(
            result,
            () => Created(
                Url.Action(
                    new UrlActionContext
                    {
                        Action = nameof(RedirectByShortUrl),
                        Values = new { shortUrl = result.Value!.ShortUrl },
                        Protocol = Request.Scheme,
                        Host = Request.Host.ToString()
                    }
                ),
                result.Value
            )
        );
    }

    /// <summary>
    /// Redirects to the original URL based on the provided short URL.
    /// </summary>
    /// <param name="shortUrl">The short URL of the original URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A redirection to the original URL if found; otherwise, a 404 Not Found response.</returns>
    [HttpGet("{shortUrl}")]
    public async Task<IActionResult> RedirectByShortUrl(
        [FromRoute] string shortUrl,
        CancellationToken cancellationToken
    )
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        
        var result = await sender.Send(
            new GetByShortUrlRequest(
                shortUrl,
                HttpContext.Request.Headers["User-Agent"].ToString(),
                HttpContext.Request.Headers["Referer"].ToString(),
                ipAddress
            ),
            cancellationToken
        );

        return HandleResult(
            result,
            () => Redirect(result.Value!.OriginalUrl!)
        );
    }
}
