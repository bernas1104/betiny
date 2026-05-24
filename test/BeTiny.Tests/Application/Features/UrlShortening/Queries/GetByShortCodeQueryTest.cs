using System.Linq.Expressions;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Features.UrlShortening.Queries.GetByShortCode;
using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using Moq;

namespace BeTiny.Tests.Application.Features.UrlShortening.Queries;

public class GetByShortCodeQueryTest
{
    private readonly Mock<IRepository<ShortUrl, ShortUrlId, Guid>> _repositoryMock;
    private readonly GetByShortCodeQuery _query;

    public GetByShortCodeQueryTest()
    {
        _repositoryMock = new Mock<IRepository<ShortUrl, ShortUrlId, Guid>>();
        _query = new GetByShortCodeQuery(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenShortCodeExists()
    {
        // Arrange
        var shortUrl = new ShortUrl("http://example.com", "abc123");

        _repositoryMock.Setup(
            repo => repo.GetByFilterAsync(
                It.Is<Expression<Func<ShortUrl, bool>>>(expr => 
                    expr.Compile()(shortUrl) == true),
                It.IsAny<CancellationToken>()
            )
        ).ReturnsAsync(shortUrl);

        var request = new GetByShortCodeRequest("abc123");

        // Act
        var result = await _query.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("http://example.com", result.Value.OriginalUrl);
        Assert.Null(result.Value.ExpiresAt);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenShortCodeDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(
            repo => repo.GetByFilterAsync(
                It.Is<Expression<Func<ShortUrl, bool>>>(expr => 
                    expr.Compile()(new ShortUrl("http://example.com", "nonexistent")) == true),
                It.IsAny<CancellationToken>()
            )
        ).ReturnsAsync((ShortUrl?)null);

        var request = new GetByShortCodeRequest("nonexistent");

        // Act
        var result = await _query.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Equal("Short code not found.", result.Errors.First().ErrorMessage);

        _repositoryMock.Verify(
            repo => repo.GetByFilterAsync(
                It.Is<Expression<Func<ShortUrl, bool>>>(expr => 
                    expr.Compile()(new ShortUrl("http://example.com", "nonexistent")) == true),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}
