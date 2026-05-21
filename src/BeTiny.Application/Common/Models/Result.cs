using System.Text.Json.Serialization;
using BeTiny.Application.Common.Enums;

namespace BeTiny.Application.Common.Models;

/// <summary>
/// Represents the result of an operation, including the value if successful or 
/// error information if failed.
/// </summary>
/// <typeparam name="TResponse">The type of the value returned by the operation.</typeparam>
public class Result<TResponse> : IResult
{
    [JsonPropertyOrder(-3)]
    public TResponse? Value { get; init; }
    object? IResult.Value => Value;
    [JsonPropertyOrder(-2)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<Error>? Errors { get; init; }
    [JsonIgnore]
    public bool IsSuccess => Errors == null || Errors.Count == 0;

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
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The errors of the failed result.</param>
    /// <returns>A <see cref="Result{TResponse}"/> representing a failed operation.</returns>
    public static Result<TResponse> Failure(params Error[] errors)
    {
        return new Result<TResponse> { Errors = errors };
    }
}
