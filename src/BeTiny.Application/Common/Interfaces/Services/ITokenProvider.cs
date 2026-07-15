using BeTiny.Application.Common.Models;
using BeTiny.Domain.ValueObjects;

namespace BeTiny.Application.Common.Interfaces.Services;

/// <summary>
/// Provides functionality to issue tokens for users.
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Issues a token for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="email">The email address of the user.</param>
    /// <returns>A <see cref="TokenResult"/> containing the issued token and its expiration date and time.</returns>
    TokenResult IssueToken(string userId, string email);
}
