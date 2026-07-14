using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using BeTiny.Domain.Common.Entities;
using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Enums;
using BeTiny.Domain.ValueObjects;

namespace BeTiny.Domain.Entities;

/// <summary>
/// Represents a registered user of the system.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed partial class User : AggregateRoot<UserId, Guid>
{
    /// <summary>
    /// Gets the normalized email address of the user.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Gets the hashed password of the user.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Gets the user's current plan.
    /// </summary>
    public Plans Plan { get; private set; }

    #pragma warning disable CS8618
    // Private empty constructor needed by EF Core
    private User()
    {
    }
    #pragma warning restore

    /// <summary>
    /// Creates a new <see cref="User"/> with the specified email and hashed password.
    /// </summary>
    /// <param name="email">The normalized email address.</param>
    /// <param name="password">The plaintext password.</param>
    /// <param name="passwordHasher">The password hasher used to hash the password.</param>
    /// <returns>A new <see cref="User"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> or <paramref name="passwordHasher"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="password"/> is invalid.</exception>
    public static User Create(Email email, string password, IPasswordHasher passwordHasher)
    {
        if (email is null)
            throw new ArgumentNullException(nameof(email));

        if (passwordHasher is null)
            throw new ArgumentNullException(nameof(passwordHasher));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));

        if (!PasswordRegex().IsMatch(password))
            throw new ArgumentException(
                "Password must be between 8 and 72 characters and contain at least one uppercase letter, one lowercase letter, and one digit.",
                nameof(password)
            );

        return new User
        {
            Id = UserId.CreateUnique(),
            Email = email,
            PasswordHash = passwordHasher.HashPassword(password),
            Plan = Plans.Free,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets the compiled regular expression used to validate passwords.
    /// </summary>
    /// <returns>A <see cref="Regex"/> for password validation.</returns>
    [GeneratedRegex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,72}$")]
    public static partial Regex PasswordRegex();
}
