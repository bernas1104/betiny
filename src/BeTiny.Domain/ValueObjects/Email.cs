using System.Text.RegularExpressions;
using BeTiny.Domain.Common.ValueObjects;

namespace BeTiny.Domain.ValueObjects;

/// <summary>
/// Represents a normalized and validated email address.
/// </summary>
public sealed partial class Email : ValueObject
{
    /// <summary>
    /// Gets the normalized email value.
    /// </summary>
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="Email"/> from the specified raw email address.
    /// </summary>
    /// <param name="email">The raw email address.</param>
    /// <returns>A new <see cref="Email"/> instance with the normalized value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="email"/> is empty, whitespace, invalid, or too long.</exception>
    public static Email Create(string email)
    {
        if (email is null)
            throw new ArgumentNullException(nameof(email));

        var normalized = email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Email cannot be empty or whitespace.", nameof(email));

        if (normalized.Length > 254)
            throw new ArgumentException("Email cannot be longer than 254 characters.", nameof(email));

        if (!EmailRegex().IsMatch(normalized))
            throw new ArgumentException("Invalid email format.", nameof(email));

        return new Email(normalized);
    }

    /// <summary>
    /// Gets the compiled regular expression used to validate email addresses.
    /// </summary>
    /// <returns>A <see cref="Regex"/> for email validation.</returns>
    [GeneratedRegex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$")]
    public static partial Regex EmailRegex();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
