using BeTiny.Application.Common.Enums;

namespace BeTiny.Application.Common.Models;

public sealed record Error(
    ErrorTypes ErrorType,
    string? PropertyName,
    string ErrorMessage,
    ErrorSeverity Severity
);
