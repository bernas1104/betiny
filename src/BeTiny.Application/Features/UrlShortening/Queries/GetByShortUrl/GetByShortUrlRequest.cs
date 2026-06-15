using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Models;
using FluentValidation;

namespace BeTiny.Application.Features.UrlShortening.Queries.GetByShortUrl;

public sealed record GetByShortUrlRequest(
    string ShortCode,
    string UserAgent,
    string Referer,
    string? IpAddress = null
)   : IRequest<Result<GetByShortUrlResponse>>;

/// <summary>
/// Validator for the <see cref="GetByShortUrlRequest"/> class.
/// </summary>
public sealed class GetByShortUrlRequestValidator : AbstractValidator<GetByShortUrlRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetByShortUrlRequestValidator"/> class.
    /// </summary>
    public GetByShortUrlRequestValidator()
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
