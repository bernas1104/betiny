using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Common.Models;
using BeTiny.Domain.Entities;
using BeTiny.Domain.Enums;
using BeTiny.Domain.Exceptions;
using BeTiny.Domain.Interfaces;
using BeTiny.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;

/// <summary>
/// Handles the creation of a short URL.
/// </summary>
public class CreateShortUrlCommand : 
    IRequestHandler<CreateShortUrlRequest, Result<CreateShortUrlResponse>>
{
    private readonly IRepository<ShortUrl, ShortUrlId, Guid> _shortUrlRepository;
    private readonly IShortCodeGenerator _shortCodeGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CreateShortUrlCommand> _logger;

    public const int MaxShortCodeGenerationAttempts = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateShortUrlCommand"/> class.
    /// </summary>
    /// <param name="shortUrlRepository">The short URL repository.</param>
    /// <param name="shortCodeGenerator">The short code generator.</param>
    /// <param name="logger">The logger.</param>
    public CreateShortUrlCommand(
        IRepository<ShortUrl, ShortUrlId, Guid> shortUrlRepository,
        IShortCodeGenerator shortCodeGenerator,
        IDateTimeProvider dateTimeProvider,
        ILogger<CreateShortUrlCommand> logger
    )
    {
        _shortUrlRepository = shortUrlRepository;
        _shortCodeGenerator = shortCodeGenerator;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Handles the creation of a short URL.
    /// </summary>
    /// <param name="command">The request containing the URL to be shortened.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the shortened URL.</returns>
    public async Task<Result<CreateShortUrlResponse>> Handle(
        CreateShortUrlRequest command,
        CancellationToken cancellationToken
    )
    {
        var created = false;
        ShortUrl shortUrl = default!;
        
        if (command.CustomAlias is not null)
        {
            shortUrl = ShortUrl.CreateFromCustomAlias(
                command.OriginalUrl,
                command.CustomAlias,
                command.ExpiresAt,
                _dateTimeProvider
            );

            created = await TryAddShortUrlAsync(shortUrl, cancellationToken);
        }
        else
        {
            var attempts = 0;
            while (!created && attempts < MaxShortCodeGenerationAttempts)
            {
                var shortCode = await _shortCodeGenerator.GenerateShortCode();
                
                shortUrl = ShortUrl.CreateFromShortCode(
                    command.OriginalUrl,
                    shortCode,
                    command.ExpiresAt,
                    _dateTimeProvider
                );
                
                created = await TryAddShortUrlAsync(shortUrl, cancellationToken);
                attempts++;
            }
        }

        if (!created)
        {
            return Result<CreateShortUrlResponse>.Failure(
                command.CustomAlias is not null
                    ? CreateConflictError(command.CustomAlias)
                    : CreateMaxAttemptsExceededError()
            );
        }

        _logger.LogInformation(
            "Short URL for {OriginalUrl} created: {AliasUrl}",
            command.OriginalUrl,
            shortUrl.AliasUrl
        );

        return Result<CreateShortUrlResponse>.Success(
            new CreateShortUrlResponse(shortUrl.AliasUrl)
        );
    }

    private async Task<bool> TryAddShortUrlAsync(ShortUrl shortUrl, CancellationToken cancellationToken)
    {
        try
        {   
            await _shortUrlRepository.AddAsync(shortUrl, cancellationToken);
            return true;
        }
        catch (DuplicateAliasUrlException ex)
        {
            _shortUrlRepository.Detach(shortUrl);
            
            _logger.LogWarning(
                ex,
                "Failed to create short URL for {OriginalUrl}. A shortened URL with the same value already exists.",
                shortUrl.OriginalUrl
            );

            if (shortUrl.Type == AliasUrlType.ShortCode)
            {
                _logger.LogInformation(
                    "Retrying short URL creation for {OriginalUrl} with a new short code.",
                    shortUrl.OriginalUrl
                );
            }

            return false;
        }
    }

    private static Error CreateConflictError(string customAlias)
    {
        return new Error(
            ErrorTypes.ConflictError,
            nameof(customAlias),
            "An URL with the same custom alias already exists.",
            ErrorSeverity.Medium
        );
    }

    private static Error CreateMaxAttemptsExceededError()
    {
        return new Error(
            ErrorTypes.ConflictError,
            null,
            "Failed to generate a unique short code after multiple attempts. Please try again.",
            ErrorSeverity.Medium
        );
    }
}
