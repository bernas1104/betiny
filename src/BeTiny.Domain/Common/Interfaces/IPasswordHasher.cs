namespace BeTiny.Domain.Common.Interfaces;

/// <summary>
/// Defines a service for hashing passwords securely.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Gets a dummy password hash used for timing attack prevention.
    /// </summary>
    string DummyPasswordHash { get; }

    /// <summary>
    /// Hashes the specified password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A hashed representation of the password.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="password"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="password"/> is empty or whitespace.</exception>
    string HashPassword(string password);

    /// <summary>
    /// Verifies that the specified password matches the hashed password.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hashedPassword">The hashed password to compare against.</param>
    /// <returns><c>true</c> if the password matches the hashed password; otherwise, <c>false</c>.</returns>
    bool VerifyPassword(string password, string hashedPassword);
}
