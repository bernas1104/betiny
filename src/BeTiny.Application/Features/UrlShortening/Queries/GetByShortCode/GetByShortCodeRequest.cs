using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Models;
using FluentValidation;

namespace BeTiny.Application.Features.UrlShortening.Queries.GetByShortCode;

/// <summary>
/// Request to get a URL by its short code.
/// </summary>
/// <param name="ShortCode">The short code of the URL.</param>
/// <returns>The result containing the URL information.</returns>
public sealed record GetByShortCodeRequest(string ShortCode)
    : IRequest<Result<GetByShortCodeResponse>>;

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
            .WithMessage("Short code must not exceed 7 characters.");
    }
}
