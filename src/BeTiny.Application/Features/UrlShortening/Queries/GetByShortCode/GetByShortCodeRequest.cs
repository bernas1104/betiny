using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Models;
using FluentValidation;

namespace BeTiny.Application.Features.UrlShortening.Queries.GetByShortCode;

public sealed record GetByShortCodeRequest(
    string ShortCode,
    string UserAgent,
    string Referer,
    string? IpAddress = null
)   : IRequest<Result<GetByShortCodeResponse>>;

/// <summary>
/// Validator for the <see cref="GetByShortCodeRequest"/> class.
/// </summary>
public sealed class GetByShortCodeRequestValidator : AbstractValidator<GetByShortCodeRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetByShortCodeRequestValidator"/> class.
    /// </summary>
    public GetByShortCodeRequestValidator()
    {
        RuleFor(x => x.ShortCode)
            .NotEmpty()
            .WithMessage("Short code must not be empty.")
            .MaximumLength(7)
            .WithMessage("Short code must not exceed 7 characters.")
            .Matches("^[a-zA-Z0-9]+$")
            .WithMessage("Short code must contain only alphanumeric characters.");
    }
}
