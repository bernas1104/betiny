using System.Diagnostics.CodeAnalysis;
using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace BeTiny.Api.Controllers;

/// <summary>
/// Base controller class for API controllers, providing common functionality 
/// and dependencies.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class Controller(ILogger<Controller> logger) : ControllerBase
{
    /// <summary>
    /// Handles the result of an operation by either returning a successful response
    /// or mapping errors to appropriate HTTP status codes.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to handle.</param>
    /// <param name="resultMethod">A function that returns the success IActionResult.</param>
    /// <returns>An IActionResult based on the result state.</returns>
    protected IActionResult HandleResult<T>(
        Result<T> result,
        Func<IActionResult> resultMethod
    )
    {
        if (result.IsSuccess)
        {
            return resultMethod();
        }

        if (result.Errors.Count == 0)
        {
            logger.LogError("An unexpected error occurred with no specific errors provided.");
            
            return new ObjectResult(
                new ProblemDetails
                {
                    Title = "InternalServerError",
                    Detail = "An unexpected error occurred.",
                    Status = 500,
                    Instance = HttpContext.Request.Path
                }
            )
            {
                StatusCode = 500
            };
        }

        var firstError = result.Errors.First();
        var statusCode = GetStatusCode(firstError.ErrorType);

        return new ObjectResult(
            new ProblemDetails
            {
                Title = firstError.ErrorType.ToString(),
                Detail = string.Join(
                    ", ",
                    result.Errors.Select(e => e.ErrorMessage)
                ),
                Status = statusCode,
                Instance = HttpContext.Request.Path
            }
        )
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Maps the specified error to an appropriate HTTP status code.
    /// </summary>
    /// <param name="error">The error to map.</param>
    /// <returns>The corresponding HTTP status code.</returns>
    private static int GetStatusCode(ErrorTypes errorType)
    {
        return errorType switch
        {
            ErrorTypes.ValidationError => 400,
            ErrorTypes.UnauthorizedError => 401,
            ErrorTypes.ForbiddenError => 403,
            ErrorTypes.NotFoundError => 404,
            ErrorTypes.ConflictError => 409,
            ErrorTypes.ExpiredError => 410,
            _ => 500
        };
    }
}
