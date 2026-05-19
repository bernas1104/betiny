using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Models;
using FluentValidation;

namespace BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;

/// <summary>
/// Represents a request to create a short URL.
/// </summary>
/// <param name="OriginalUrl">The original URL to be shortened.</param>
/// <returns>A response containing the shortened URL.</returns>
public sealed record CreateShortUrlRequest(string OriginalUrl)
    : ICommand<Result<CreateShortUrlResponse>>;

public sealed class CreateShortUrlRequestValidator : AbstractValidator<CreateShortUrlRequest>
{
    public CreateShortUrlRequestValidator()
    {
        RuleFor(x => x.OriginalUrl)
            .NotEmpty()
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("The OriginalUrl must be a valid absolute URL.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out var parsed)
                && (
                    parsed.Scheme.Equals(Uri.UriSchemeHttp)
                    || parsed.Scheme.Equals(Uri.UriSchemeHttps)
                ))
            .WithMessage("The OriginalUrl must use HTTP or HTTPS scheme.");
    }
}
