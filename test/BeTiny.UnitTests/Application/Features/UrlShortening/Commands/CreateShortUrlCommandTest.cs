using System.Linq.Expressions;
using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;
using BeTiny.Domain.Entities;
using BeTiny.Domain.Interfaces;
using BeTiny.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BeTiny.UnitTests.Application.Features.UrlShortening.Commands;

public class CreateShortUrlCommandTest
{
    private readonly IRepository<ShortUrl, ShortUrlId, Guid> _shortUrlRepository;
    private readonly IShortCodeGenerator _shortCodeGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CreateShortUrlCommand> _logger;
    private readonly CreateShortUrlCommand _handler;

    public CreateShortUrlCommandTest()
    {
        _shortUrlRepository = Substitute.For<IRepository<ShortUrl, ShortUrlId, Guid>>();
        _shortCodeGenerator = Substitute.For<IShortCodeGenerator>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _logger = Substitute.For<ILogger<CreateShortUrlCommand>>();

        _handler = new CreateShortUrlCommand(
            _shortUrlRepository,
            _shortCodeGenerator,
            _dateTimeProvider,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WhenNoCustomAliasProvided_ShouldCreateShortUrlFromShortCodeGenerator()
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
                    && s.CustomAlias == null
            ),
            Arg.Any<CancellationToken>()
        );

        await _shortUrlRepository.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCustomAliasProvided_ShouldCreateShortUrlWithCustomAlias()
    {
        // Arrange
        var originalUrl = "https://www.example.com";
        var customAlias = "my-alias";

        var request = new CreateShortUrlRequest(originalUrl, customAlias);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ShortUrl.Should().Be(customAlias);

        await _shortCodeGenerator.DidNotReceive().GenerateShortCode();

        await _shortUrlRepository.Received(1)
            .AddAsync(
                Arg.Is<ShortUrl>(
                    s => s.OriginalUrl == originalUrl
                        && s.CustomAlias == customAlias
                        && s.ShortCode == null
                ),
                Arg.Any<CancellationToken>()
            );

        await _shortUrlRepository.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCustomAliasAlreadyExists_ShouldReturnFailureResult()
    {
        // Arrange
        var originalUrl = "https://www.example.com";
        var customAlias = "existing-alias";

        _shortUrlRepository.AnyAsync(
            Arg.Any<Expression<Func<ShortUrl, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(true);

        var request = new CreateShortUrlRequest(originalUrl, customAlias);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorType == ErrorTypes.ConflictError);
    }
}
