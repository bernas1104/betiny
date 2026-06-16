using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Models;
using FluentValidation;

namespace BeTiny.Application.Features.UrlShortening.Queries.GetByShortUrl;

/// <summary>
/// Represents a request to retrieve the original URL associated with a short URL.
/// </summary>
/// <param name="ShortUrl">The short URL to look up.</param>
/// <param name="UserAgent">The user agent string from the HTTP request.</param>
/// <param name="Referer">The referer header from the HTTP request.</param>
/// <param name="IpAddress">The IP address of the client, if available.</param>
public sealed record GetByShortUrlRequest(
    string ShortUrl,
    string UserAgent,
    string Referer,
    string? IpAddress = null
)   : IQuery<Result<GetByShortUrlResponse>>;

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
        RuleFor(x => x.ShortUrl)
            .NotEmpty()
            .WithMessage("Short URL must not be empty.")
            .MaximumLength(50)
            .WithMessage("Short URL must not exceed 50 characters.")
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage("Short URL must contain only alphanumeric characters, hyphens, or underscores.");
    }
}
