using System.Text.Json;

using BeTiny.Api.Application.Common.Enums;
using BeTiny.Api.Application.Common.Models;

using Microsoft.AspNetCore.Mvc;

namespace BeTiny.Api.Application.Common.Helpers
{
    public static class ResultsHelper
    {
        private static readonly JsonSerializerOptions _options =
            new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public static IResult GetResultFromError<T>(HttpContext context, Result<T> result)
        {
            var problemDetails = new ProblemDetails()
            {
                Title = "An error occurred",
                Detail = result.ErrorMessage,
                Instance = context.Request.Path
            };

            return MapResultsFromError(result.Error, problemDetails);
        }

        private static IResult MapResultsFromError(Error? error, ProblemDetails details)
        {
            int statusCode = error switch
            {
                Error.NotFound => StatusCodes.Status404NotFound,
                Error.Validation => StatusCodes.Status400BadRequest,
                Error.Infrastructure => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status500InternalServerError
            };

            details.Status = statusCode;

            return Results.Json(
                details,
                _options,
                statusCode: statusCode
            );
        }
    }
}
