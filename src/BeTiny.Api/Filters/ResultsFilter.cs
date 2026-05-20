using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BeTiny.Api.Filters;

/// <summary>
/// Filter to handle results and ensure consistent response formatting.
/// </summary>
public class ResultsFilter : IResultFilter
{
    /// <summary>
    /// Called before the action result executes.
    /// It checks if the result is of type <see cref="IResult"/> and formats 
    /// the response accordingly.
    /// If the result indicates an error, it returns a <see cref="ProblemDetails"/> 
    /// response with appropriate status code and details.
    /// If the result is successful, it returns the value with the original status code.
    /// </summary>
    /// <param name="context">The context for the result execution.</param>
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (
            context.Result is ObjectResult objectResult
                && objectResult.Value is Application.Common.Models.IResult result
        )
        {
            if (result.Error is null)
            {
                context.Result = new ObjectResult(result.Value)
                {
                    StatusCode = objectResult.StatusCode
                };
                
                return;
            }

            var status = GetStatusCode(result.Error.Value);

            context.Result = new ObjectResult(
                new ProblemDetails
                {
                    Title = result.Error.ToString(),
                    Detail = result.ErrorMessage,
                    Status = status,
                    Instance = context.HttpContext.Request.Path
                }
            )
            {
                StatusCode = status
            };
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // noop
    }

    /// <summary>
    /// Maps the specified error to an appropriate HTTP status code.
    /// </summary>
    /// <param name="error">The error to map.</param>
    /// <returns>The corresponding HTTP status code.</returns>
    private static int GetStatusCode(Errors error)
    {
        return error switch
        {
            Errors.ValidationError => 400,
            Errors.UnexpectedError => 500,
            _ => 500
        };
    }
}
