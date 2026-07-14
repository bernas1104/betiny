using BeTiny.Domain.Common.Interfaces;

namespace BeTiny.Infrastructure.Services;

/// <summary>
/// Hashes passwords using the BCrypt algorithm.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    /// <inheritdoc/>
    public string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));

        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);
    }
}
