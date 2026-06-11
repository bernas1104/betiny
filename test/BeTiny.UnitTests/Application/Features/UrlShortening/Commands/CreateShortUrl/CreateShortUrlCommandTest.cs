using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;
using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BeTiny.UnitTests.Application.Features.UrlShortening.Commands.CreateShortUrl;

public class CreateShortUrlCommandTest
{
    private readonly IRepository<ShortUrl, ShortUrlId, Guid> _shortUrlRepository;
    private readonly IShortCodeGenerator _shortCodeGenerator;
    private readonly ILogger<CreateShortUrlCommand> _logger;
    private readonly CreateShortUrlCommand _handler;

    public CreateShortUrlCommandTest()
    {
        _shortUrlRepository = Substitute.For<IRepository<ShortUrl, ShortUrlId, Guid>>();
        _shortCodeGenerator = Substitute.For<IShortCodeGenerator>();
        _logger = Substitute.For<ILogger<CreateShortUrlCommand>>();

        _handler = new CreateShortUrlCommand(
            _shortUrlRepository,
            _shortCodeGenerator,
            _logger
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateShortUrl()
    {
        // Arrange
        var originalUrl = "https://www.example.com";
        var shortCode = "abc123";
        _shortCodeGenerator.GenerateShortCode()
            .Returns(shortCode);

        var request = new CreateShortUrlRequest(originalUrl);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ShortUrl.Should().Be(shortCode);

        await _shortUrlRepository.Received(1).AddAsync(
            Arg.Is<ShortUrl>(
                s => s.OriginalUrl == originalUrl
                    && s.ShortCode == shortCode
            ),
            Arg.Any<CancellationToken>()
        );

        await _shortUrlRepository.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }
}
