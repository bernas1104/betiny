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
public abstract class Controller : ControllerBase
{
    protected readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="Controller"/> class with 
    /// the specified sender.
    /// </summary>
    /// <param name="sender">The sender used for sending commands and queries.</param>
    protected Controller(ISender sender)
    {
        _sender = sender;
    }

    protected IActionResult HandleResult<T>(
        Result<T> result,
        Func<IActionResult> resultMethod
    )
    {
        if (result.IsSuccess)
        {
            return resultMethod();
        }

        var firstError = result.Errors!.First();
        var statusCode = GetStatusCode(firstError.ErrorType);

        return new ObjectResult(
            new ProblemDetails
            {
                Title = firstError.ErrorType.ToString(),
                Detail = string.Join(
                    ", ",
                    result.Errors!.Select(e => e.ErrorMessage)
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
            ErrorTypes.NotFoundError => 404,
            ErrorTypes.ExpiredError => 410,
            _ => 500
        };
    }
}
