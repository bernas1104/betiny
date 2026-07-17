using BeTiny.Domain.Enums;
using BeTiny.Domain.ValueObjects;

namespace BeTiny.Domain.Common.Interfaces;

/// <summary>
/// Represents the current authenticated user.
/// </summary>
public interface ICurrentUser
{
    UserId? UserId { get; }
    string? Email { get; }
    Plans? Plan { get; }
    bool IsAuthenticated { get; }
}
