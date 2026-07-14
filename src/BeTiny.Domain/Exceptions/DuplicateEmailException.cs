namespace BeTiny.Domain.Exceptions;

/// <summary>
/// Thrown when an attempt is made to create a user with an email that already exists.
/// </summary>
public sealed class DuplicateEmailException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateEmailException"/> class with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public DuplicateEmailException(string message) : base(message)
    {
    }
}
