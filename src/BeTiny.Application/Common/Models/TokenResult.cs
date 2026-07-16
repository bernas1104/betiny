namespace BeTiny.Application.Common.Models;

/// <summary>
/// Represents the result of a token issuance.
/// </summary>
/// <param name="Token">The issued token.</param>
/// <param name="ExpiresAt">The expiration date and time of the token.</param>
public sealed record TokenResult(string Token, DateTime ExpiresAt);
