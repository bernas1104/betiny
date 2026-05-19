using BeTiny.Application.Common.Enums;

namespace BeTiny.Application.Common.Models;

/// <summary>
/// Represents the result of an operation, including the value if successful or 
/// error information if failed.
/// </summary>
/// <typeparam name="TResponse">The type of the value returned by the operation.</typeparam>
public class Result<TResponse>
{
    public TResponse? Value { get; set; }
    public Errors? Error { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSuccess => Error is null;

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value of the successful result.</param>
    /// <returns>A <see cref="Result{TResponse}"/> representing a successful operation.</returns>
    public static Result<TResponse> Success(TResponse value)
    {
        return new Result<TResponse> { Value = value };
    }

    /// <summary>
    /// Creates a failed result with the specified error and optional error message.
    /// </summary>
    /// <param name="error">The error type of the failed result.</param>
    /// <param name="errorMessage">An optional error message providing additional details.</param>
    /// <returns>A <see cref="Result{TResponse}"/> representing a failed operation.</returns>
    public static Result<TResponse> Failure(Errors error, string? errorMessage = null)
    {
        return new Result<TResponse> { Error = error, ErrorMessage = errorMessage };
    }
}
