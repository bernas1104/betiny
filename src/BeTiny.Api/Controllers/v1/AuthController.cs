using System.Diagnostics.CodeAnalysis;
using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Features.Auth.Commands.Login;
using BeTiny.Application.Features.Auth.Commands.Register;
using BeTiny.Application.Features.Auth.Queries.GetMe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeTiny.Api.Controllers.v1;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[ExcludeFromCodeCoverage]
public class AuthController(ISender sender, ILogger<AuthController> logger) : Controller(logger)
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request containing email and password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A 201 Created response with the registered user details.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(request, cancellationToken);

        return HandleResult(
            result,
            () => Created($"/api/v1/auth/users/{result.Value!.Id}", result.Value)
        );
    }

    /// <summary>
    /// Logs in a user.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A 200 OK response with the authentication token.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(request, cancellationToken);

        return HandleResult(
            result,
            () => Ok(result.Value)
        );
    }

    /// <summary>
    /// Gets the currently authenticated user's details.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A 200 OK response with the authenticated user's details.</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMeRequest(), cancellationToken);

        return HandleResult(
            result,
            () => Ok(result.Value)
        );
    }
}
