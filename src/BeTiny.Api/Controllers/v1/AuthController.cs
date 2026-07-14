using System.Diagnostics.CodeAnalysis;
using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Features.Auth.Commands.Register;
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
}
