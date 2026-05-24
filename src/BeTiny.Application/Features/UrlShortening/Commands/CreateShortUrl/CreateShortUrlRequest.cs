using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Models;
using FluentValidation;

namespace BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;

/// <summary>
/// Represents a request to create a short URL.
/// </summary>
/// <param name="OriginalUrl">The original URL to be shortened.</param>
/// <param name="ExpiresAt">The optional expiration date and time for the short URL.</param>
/// <returns>A response containing the shortened URL.</returns>
public sealed record CreateShortUrlRequest(
    string OriginalUrl,
    DateTime? ExpiresAt = null
) : ICommand<Result<CreateShortUrlResponse>>;

public sealed class CreateShortUrlRequestValidator : AbstractValidator<CreateShortUrlRequest>
{
    public CreateShortUrlRequestValidator()
    {
        RuleFor(x => x.OriginalUrl)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out var parsed) 
                && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps))
            .WithMessage("The OriginalUrl must be a valid absolute URL using HTTP or HTTPS scheme.");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpiresAt.HasValue)
            .WithMessage("The ExpiresAt must be a future date and time.");
    }
}
