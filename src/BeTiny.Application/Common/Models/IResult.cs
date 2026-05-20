using BeTiny.Application.Common.Enums;

namespace BeTiny.Application.Common.Models;

/// <summary>
/// Represents a non-generic result of an operation that can indicate success or failure.
/// </summary>
public interface IResult
{
    public object? Value { get; }
    Errors? Error { get; set; }
    string? ErrorMessage { get; set; }
}
