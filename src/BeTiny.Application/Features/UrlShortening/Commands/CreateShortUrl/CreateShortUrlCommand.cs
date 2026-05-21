using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Common.Models;
using BeTiny.Domain.Entities;
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
    private readonly ILogger<CreateShortUrlCommand> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateShortUrlCommand"/> class.
    /// </summary>
    /// <param name="shortUrlRepository">The short URL repository.</param>
    /// <param name="shortCodeGenerator">The short code generator.</param>
    /// <param name="logger">The logger.</param>
    public CreateShortUrlCommand(
        IRepository<ShortUrl, ShortUrlId, Guid> shortUrlRepository,
        IShortCodeGenerator shortCodeGenerator,
        ILogger<CreateShortUrlCommand> logger
    )
    {
        _shortUrlRepository = shortUrlRepository;
        _shortCodeGenerator = shortCodeGenerator;
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
        var shortCode = await _shortCodeGenerator.GenerateShortCode();

        var shortUrl = new ShortUrl(
            command.OriginalUrl,
            shortCode
        );

        await _shortUrlRepository.AddAsync(shortUrl, cancellationToken);
        await _shortUrlRepository.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "Short URL for {OriginalUrl} created: {ShortUrl}",
            command.OriginalUrl,
            shortUrl.ShortCode
        );

        return Result<CreateShortUrlResponse>.Success(
            new CreateShortUrlResponse(shortUrl.ShortCode)
        );
    }
}
