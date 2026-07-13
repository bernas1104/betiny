using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Common.Models;
using BeTiny.Domain.Common.Interfaces;
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
    private readonly IReservedAliasPolicy _reservedAliasPolicy;
    private readonly ILogger<CreateShortUrlCommand> _logger;

    public const int MaxShortCodeGenerationAttempts = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateShortUrlCommand"/> class.
    /// </summary>
    /// <param name="shortUrlRepository">The short URL repository.</param>
    /// <param name="shortCodeGenerator">The short code generator.</param>
    /// <param name="dateTimeProvider">The date and time provider.</param>
    /// <param name="reservedAliasPolicy">The reserved alias policy.</param>
    /// <param name="logger">The logger.</param>
    public CreateShortUrlCommand(
        IRepository<ShortUrl, ShortUrlId, Guid> shortUrlRepository,
        IShortCodeGenerator shortCodeGenerator,
        IDateTimeProvider dateTimeProvider,
        IReservedAliasPolicy reservedAliasPolicy,
        ILogger<CreateShortUrlCommand> logger
    )
    {
        _shortUrlRepository = shortUrlRepository;
        _shortCodeGenerator = shortCodeGenerator;
        _dateTimeProvider = dateTimeProvider;
        _reservedAliasPolicy = reservedAliasPolicy;
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
        var result = command.CustomAlias is not null
            ? await CreateWithCustomAliasAsync(command, cancellationToken)
            : await CreateWithShortCodeAsync(command, cancellationToken);

        if (!result.IsSuccess)
            return Result<CreateShortUrlResponse>.Failure([..result.Errors]);

        _logger.LogInformation(
            "Short URL for {OriginalUrl} created: {AliasUrl}",
            command.OriginalUrl,
            result.Value!.AliasUrl
        );

        return Result<CreateShortUrlResponse>.Success(
            new CreateShortUrlResponse(result.Value!.AliasUrl)
        );
    }

    private async Task<Result<ShortUrl>> CreateWithCustomAliasAsync(
        CreateShortUrlRequest command,
        CancellationToken cancellationToken
    )
    {
        ShortUrl shortUrl;
        try
        {
            shortUrl = ShortUrl.CreateFromCustomAlias(
                command.OriginalUrl,
                command.CustomAlias!,
                command.ExpiresAt,
                _dateTimeProvider,
                _reservedAliasPolicy
            );
        }
        catch (ReservedAliasException ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to create short URL for {OriginalUrl}. The custom alias '{CustomAlias}' is reserved.",
                command.OriginalUrl,
                command.CustomAlias
            );

            return Result<ShortUrl>.Failure(CreateReservedAliasError(command.CustomAlias!));
        }

        return await TryAddShortUrlAsync(shortUrl, cancellationToken)
            ? Result<ShortUrl>.Success(shortUrl)
            : Result<ShortUrl>.Failure(CreateConflictError(command.CustomAlias!));
    }

    private async Task<Result<ShortUrl>> CreateWithShortCodeAsync(
        CreateShortUrlRequest command,
        CancellationToken cancellationToken
    )
    {
        for (int attempts = 0; attempts < MaxShortCodeGenerationAttempts; attempts++)
        {
            var shortCode = await _shortCodeGenerator.GenerateShortCode();

            if (_reservedAliasPolicy.IsReserved(shortCode))
            {
                _logger.LogWarning(
                    "Generated short code '{ShortCode}' is reserved. Retrying...",
                    shortCode
                );

                continue;
            }

            var shortUrl = ShortUrl.CreateFromShortCode(
                command.OriginalUrl,
                shortCode,
                command.ExpiresAt,
                _dateTimeProvider,
                _reservedAliasPolicy
            );

            if (await TryAddShortUrlAsync(shortUrl, cancellationToken))
                return Result<ShortUrl>.Success(shortUrl);
        }

        return Result<ShortUrl>.Failure(CreateMaxAttemptsExceededError());
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
            $"An URL with the same custom alias ('{customAlias}') already exists.",
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

    private static Error CreateReservedAliasError(string alias)
    {
        return new Error(
            ErrorTypes.ValidationError,
            "CustomAlias",
            $"The alias '{alias}' is reserved and cannot be used.",
            ErrorSeverity.Low
        );
    }
}
