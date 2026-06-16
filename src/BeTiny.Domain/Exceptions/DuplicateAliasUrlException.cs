namespace BeTiny.Domain.Exceptions;

public sealed class DuplicateAliasUrlException : Exception
{
    public DuplicateAliasUrlException(string message) : base(message)
    {
    }
}
