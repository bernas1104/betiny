namespace BeTiny.Domain.Exceptions;

/// <summary>
/// Thrown when an attempt is made to create a short URL with an alias that already exists.
/// </summary>
public sealed class DuplicateAliasUrlException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateAliasUrlException"/> class with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public DuplicateAliasUrlException(string message) : base(message)
    {
    }
}
