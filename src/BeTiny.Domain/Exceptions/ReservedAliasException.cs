namespace BeTiny.Domain.Exceptions;

/// <summary>
/// Represents an error that occurs when attempting to use a reserved alias.
/// </summary>
public sealed class ReservedAliasException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReservedAliasException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ReservedAliasException(string message)
        : base(message)
    {
    }
}
