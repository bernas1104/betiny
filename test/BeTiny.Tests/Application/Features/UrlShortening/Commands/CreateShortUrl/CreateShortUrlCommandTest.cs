using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;
using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace BeTiny.Tests.Application.Features.UrlShortening.Commands.CreateShortUrl;

public class CreateShortUrlCommandTest
{
    private readonly Mock<IRepository<ShortUrl, ShortUrlId, Guid>> _shortUrlRepositoryMock;
    private readonly Mock<IShortCodeGenerator> _shortCodeGeneratorMock;
    private readonly Mock<ILogger<CreateShortUrlCommand>> _loggerMock;
    private readonly CreateShortUrlCommand _handler;

    public CreateShortUrlCommandTest()
    {
        _shortUrlRepositoryMock = new Mock<IRepository<ShortUrl, ShortUrlId, Guid>>();
        _shortCodeGeneratorMock = new Mock<IShortCodeGenerator>();
        _loggerMock = new Mock<ILogger<CreateShortUrlCommand>>();

        _handler = new CreateShortUrlCommand(
            _shortUrlRepositoryMock.Object,
            _shortCodeGeneratorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateShortUrl()
    {
        // Arrange
        var originalUrl = "https://www.example.com";
        var shortCode = "abc123";
        _shortCodeGeneratorMock.Setup(x => x.GenerateShortCode())
            .ReturnsAsync(shortCode);

        var request = new CreateShortUrlRequest(originalUrl);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(shortCode, result.Value.ShortUrl);
        
        _shortUrlRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<ShortUrl>(
                    s => s.OriginalUrl == originalUrl
                        && s.ShortCode == shortCode
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _shortUrlRepositoryMock.Verify(
            x => x.SaveChanges(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}