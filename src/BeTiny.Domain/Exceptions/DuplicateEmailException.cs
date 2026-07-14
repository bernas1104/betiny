namespace BeTiny.Domain.Exceptions;

/// <summary>
/// Thrown when an attempt is made to create a user with an email that already exists.
/// </summary>
public sealed class DuplicateEmailException(string message) : Exception(message);
