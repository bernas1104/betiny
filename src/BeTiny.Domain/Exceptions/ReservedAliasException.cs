namespace BeTiny.Domain.Exceptions;

/// <summary>
/// Represents an error that occurs when attempting to use a reserved alias.
/// </summary>
public sealed class ReservedAliasException(string message) : Exception(message);
