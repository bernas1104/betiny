using System.Diagnostics.CodeAnalysis;
using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;
using BeTiny.Application.Features.UrlShortening.Queries.GetByShortCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace BeTiny.Api.Controllers.v1;

/// <summary>
/// Controller for URL shortening operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[ExcludeFromCodeCoverage]
public class UrlShortenerController : Controller
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UrlShortenerController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to send commands and queries.</param>
    public UrlShortenerController(ISender sender)
        : base(sender)
    {
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
        return HandleResult(
            result,
            () => Created(
                Url.Action(
                    new UrlActionContext
                    {
                        Action = nameof(RedirectByShortCode),
                        Values = new { shortCode = result.Value!.ShortUrl },
                        Protocol = Request.Scheme,
                        Host = Request.Host.ToString()
                    }
                ),
                result.Value
            )
        );
    }

    /// <summary>
    /// Redirects to the original URL based on the provided short code.
    /// </summary>
    /// <param name="shortCode">The short code of the URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A redirection to the original URL if found; otherwise, a 404 Not Found response.</returns>
    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectByShortCode(
        [FromRoute] string shortCode,
        CancellationToken cancellationToken
    )
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        
        var result = await _sender.Send(
            new GetByShortCodeRequest(
                shortCode,
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
