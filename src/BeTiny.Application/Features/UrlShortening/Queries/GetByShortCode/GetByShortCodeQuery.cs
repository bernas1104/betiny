using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Models;
using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;

namespace BeTiny.Application.Features.UrlShortening.Queries.GetByShortCode;

/// <summary>
/// Query to get a URL by its short code.
/// </summary>
public class GetByShortCodeQuery
    : IRequestHandler<GetByShortCodeRequest, Result<GetByShortCodeResponse>>
{
    private readonly IRepository<ShortUrl, ShortUrlId, Guid> _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetByShortCodeQuery"/> class.
    /// </summary>
    /// <param name="repository">The repository for URL shortening.</param>
    public GetByShortCodeQuery(IRepository<ShortUrl, ShortUrlId, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the query to get a URL by its short code.
    /// </summary>
    /// <param name="request">The request containing the short code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the URL information.</returns>
    public async Task<Result<GetByShortCodeResponse>> Handle(
        GetByShortCodeRequest request,
        CancellationToken cancellationToken
    )
    {
        var shortUrl = await _repository.GetByFilterAsync(
            x => x.ShortCode.Equals(request.ShortCode),
            cancellationToken
        );

        return shortUrl is null
            ? Result<GetByShortCodeResponse>.Failure(
                new Error(
                    ErrorTypes.NotFoundError,
                    null,
                    "Short code not found.",
                    ErrorSeverity.Medium
                )
            )
            : Result<GetByShortCodeResponse>.Success(
                new GetByShortCodeResponse(
                    shortUrl.OriginalUrl,
                    shortUrl.ExpiresAt
                )
            );
    }
}
