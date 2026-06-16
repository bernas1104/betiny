using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Models;
using FluentValidation;

namespace BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;

/// <summary>
/// Represents a request to create a short URL.
/// </summary>
/// <param name="OriginalUrl">The original URL to be shortened.</param>
/// <param name="CustomAlias">The optional custom alias for the short URL.</param>
/// <param name="ExpiresAt">The optional expiration date and time for the short URL.</param>
/// <returns>A response containing the shortened URL.</returns>
public sealed record CreateShortUrlRequest(
    string OriginalUrl,
    string? CustomAlias = null,
    DateTime? ExpiresAt = null
) : ICommand<Result<CreateShortUrlResponse>>;

/// <summary>
/// Validates <see cref="CreateShortUrlRequest"/> instances.
/// </summary>
public sealed class CreateShortUrlRequestValidator : AbstractValidator<CreateShortUrlRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateShortUrlRequestValidator"/> class.
    /// </summary>
    public CreateShortUrlRequestValidator()
    {
        RuleFor(x => x.OriginalUrl)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out var parsed) 
                && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps))
            .WithMessage("The OriginalUrl must be a valid absolute URL using HTTP or HTTPS scheme.");

        RuleFor(x => x.CustomAlias)
            .NotEmpty()
            .WithMessage("The CustomAlias must not be empty if provided.")
            .MinimumLength(3)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage("The CustomAlias can only contain letters, numbers, underscores, and hyphens.")
            .When(x => x.CustomAlias is not null);

        RuleFor(x => x.ExpiresAt)
            .Must(dt => !dt.HasValue || dt.Value.Kind == DateTimeKind.Utc)
            .WithMessage("The ExpiresAt must be in UTC.")
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("The ExpiresAt must be a future date and time.")
            .When(x => x.ExpiresAt.HasValue);
    }
}
