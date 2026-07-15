using BeTiny.Domain.Common.Interfaces;

namespace BeTiny.Infrastructure.Services;

/// <summary>
/// Hashes passwords using the BCrypt algorithm.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    /// <inheritdoc/>
    public string DummyPasswordHash { get => _dummyPasswordHash; }

    private const int WorkFactor = 12;
    private static readonly string _dummyPasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword("dummy", WorkFactor);

    /// <inheritdoc/>
    public string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));

        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="password"/> or <paramref name="hashedPassword"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="password"/> or <paramref name="hashedPassword"/> is empty or whitespace.</exception>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));

        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException("Hashed password cannot be null or whitespace.", nameof(hashedPassword));

        return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
    }
}
