using BeTiny.Api.Application.Common.Enums;

namespace BeTiny.Api.Application.Common.Models
{
    public class Result<T>
    {
        public T? Value { get; init; }
        public Error? Error { get; init; }
        public string? ErrorMessage { get; init; }
        public bool IsSuccess { get => Error is null; }

        public static Result<T> Success(T value)
        {
            return new Result<T>() { Value = value };
        }

        public static Result<T> Failure(Error error, string? message = null)
        {
            return new Result<T>()
            {
                Error = error,
                ErrorMessage = message
            };
        }
    }
}
