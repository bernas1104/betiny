using BeTiny.Application.Common.Models;

namespace BeTiny.Application.Common.Interfaces.Cqrs.Contracts;

/// <summary>
/// Represents a non-generic result of an operation that can indicate success or failure.
/// </summary>
public interface IResult
{
    IReadOnlyCollection<Error> Errors { get; init; }
    bool IsSuccess { get; }
}
