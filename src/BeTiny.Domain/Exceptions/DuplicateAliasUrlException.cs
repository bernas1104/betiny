namespace BeTiny.Domain.Exceptions;

/// <summary>
/// Thrown when an attempt is made to create a short URL with an alias that already exists.
/// </summary>
public sealed class DuplicateAliasUrlException(string message) : Exception(message);
