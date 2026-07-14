namespace BeTiny.Domain.Common.Interfaces;

/// <summary>
/// Defines a service for hashing passwords securely.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes the specified password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A hashed representation of the password.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="password"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="password"/> is empty or whitespace.</exception>
    string HashPassword(string password);
}
