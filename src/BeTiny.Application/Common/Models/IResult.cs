namespace BeTiny.Application.Common.Models;

/// <summary>
/// Represents a non-generic result of an operation that can indicate success or failure.
/// </summary>
public interface IResult
{
    object? Value { get; }
    IReadOnlyCollection<Error>? Errors { get; init; }
    bool IsSuccess { get; }
}
