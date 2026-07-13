using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;
using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Entities;
using BeTiny.Domain.Exceptions;
using BeTiny.Domain.Interfaces;
using BeTiny.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

namespace BeTiny.UnitTests.Application.Features.UrlShortening.Commands;

public class CreateShortUrlCommandTest
{
    private readonly IRepository<ShortUrl, ShortUrlId, Guid> _shortUrlRepository;
    private readonly IShortCodeGenerator _shortCodeGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CreateShortUrlCommand> _logger;
    private readonly IReservedAliasPolicy _reservedAliasPolicy;
    private readonly CreateShortUrlCommand _handler;

    public CreateShortUrlCommandTest()
    {
        _shortUrlRepository = Substitute.For<IRepository<ShortUrl, ShortUrlId, Guid>>();
        _shortCodeGenerator = Substitute.For<IShortCodeGenerator>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _reservedAliasPolicy = Substitute.For<IReservedAliasPolicy>();

        _logger = Substitute.For<ILogger<CreateShortUrlCommand>>();

        _handler = new CreateShortUrlCommand(
            _shortUrlRepository,
            _shortCodeGenerator,
            _dateTimeProvider,
            _reservedAliasPolicy,
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
                    && s.AliasUrl == shortCode
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_WhenShortCodeThrowsDuplicateAliasUrlException_ShouldRetryWithNewShortCode()
    {
        // Arrange
        var originalUrl = "https://www.example.com";
        var firstShortCode = "abc123";
        var secondShortCode = "def456";

        _shortCodeGenerator.GenerateShortCode()
            .Returns(firstShortCode, secondShortCode);

        _shortUrlRepository.AddAsync(Arg.Any<ShortUrl>(), Arg.Any<CancellationToken>())
            .Returns(
                _ => throw new DuplicateAliasUrlException("A shortened URL with the same value already exists."),
                _ => Task.CompletedTask
            );

        var request = new CreateShortUrlRequest(originalUrl);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ShortUrl.Should().Be(secondShortCode);

        await _shortCodeGenerator.Received(2).GenerateShortCode();

        await _shortUrlRepository.Received(2)
            .AddAsync(
                Arg.Is<ShortUrl>(
                    s => s.OriginalUrl == originalUrl
                        && (s.AliasUrl == firstShortCode || s.AliasUrl == secondShortCode)
                ),
                Arg.Any<CancellationToken>()
            );

        _shortUrlRepository.Received(1).Detach(Arg.Any<ShortUrl>());

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Warning)
            .Should()
            .ContainSingle();

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Information)
            .Should()
            .HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenShortCodeGeneratorGeneratesReservedAlias_ShouldRetryWithNewShortCode()
    {
        // Arrange
        var originalUrl = "https://www.example.com";
        var reservedShortCode = "reserved";
        var validShortCode = "valid";

        _shortCodeGenerator.GenerateShortCode()
            .Returns(reservedShortCode, validShortCode);

        _reservedAliasPolicy.IsReserved(reservedShortCode).Returns(true);
        _reservedAliasPolicy.IsReserved(validShortCode).Returns(false);

        var request = new CreateShortUrlRequest(originalUrl);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ShortUrl.Should().Be(validShortCode);

        await _shortCodeGenerator.Received(2).GenerateShortCode();

        await _shortUrlRepository.Received(1)
            .AddAsync(
                Arg.Is<ShortUrl>(
                    s => s.OriginalUrl == originalUrl
                        && s.AliasUrl == validShortCode
                ),
                Arg.Any<CancellationToken>()
            );

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Warning)
            .Should()
            .ContainSingle();

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Information)
            .Should()
            .ContainSingle();
    }

    [Fact]
    public async Task Handle_WhenShortCodeThrowsDuplicateAliasUrlExceptionMultipleTimes_ReturnsFailureResult()
    {
        // Arrange
        var originalUrl = "https://www.example.com";
        var shortCode = "abc123";

        _shortCodeGenerator.GenerateShortCode()
            .Returns(shortCode);

        _shortUrlRepository.AddAsync(Arg.Any<ShortUrl>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(
                new DuplicateAliasUrlException("A shortened URL with the same value already exists.")
            );

        var request = new CreateShortUrlRequest(originalUrl);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorType == ErrorTypes.ConflictError);

        await _shortCodeGenerator.Received(CreateShortUrlCommand.MaxShortCodeGenerationAttempts)
            .GenerateShortCode();

        await _shortUrlRepository.Received(CreateShortUrlCommand.MaxShortCodeGenerationAttempts)
            .AddAsync(
                Arg.Is<ShortUrl>(
                    s => s.OriginalUrl == originalUrl
                        && s.AliasUrl == shortCode
                ),
                Arg.Any<CancellationToken>()
            );

        _shortUrlRepository.Received(CreateShortUrlCommand.MaxShortCodeGenerationAttempts)
            .Detach(Arg.Any<ShortUrl>());

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Warning)
            .Should()
            .HaveCount(CreateShortUrlCommand.MaxShortCodeGenerationAttempts);

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Information)
            .Should()
            .HaveCount(CreateShortUrlCommand.MaxShortCodeGenerationAttempts);
    }

    [Fact]
    public async Task Handle_WhenShortCodeGeneratorGeneratesReservedAliasMultipleTimes_ReturnsFailureResult()
    {
        // Arrange
        var originalUrl = "https://www.example.com";
        var reservedShortCode = "reserved";

        _shortCodeGenerator.GenerateShortCode()
            .Returns(reservedShortCode);

        _reservedAliasPolicy.IsReserved(reservedShortCode).Returns(true);

        var request = new CreateShortUrlRequest(originalUrl);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorType == ErrorTypes.ConflictError);

        await _shortCodeGenerator.Received(CreateShortUrlCommand.MaxShortCodeGenerationAttempts)
            .GenerateShortCode();

        await _shortUrlRepository.DidNotReceive()
            .AddAsync(Arg.Any<ShortUrl>(), Arg.Any<CancellationToken>());

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Warning)
            .Should()
            .HaveCount(CreateShortUrlCommand.MaxShortCodeGenerationAttempts);
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
                        && s.AliasUrl == customAlias
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenAliasUrlAlreadyExists_ShouldReturnFailureResult()
    {
        // Arrange
        var originalUrl = "https://www.example.com";
        var customAlias = "existing-alias";

        _shortUrlRepository.AddAsync(Arg.Any<ShortUrl>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(
                new DuplicateAliasUrlException("A custom alias already exists for the provided value.")
            );

        var request = new CreateShortUrlRequest(originalUrl, customAlias);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorType == ErrorTypes.ConflictError);

        await _shortUrlRepository.Received(1)
            .AddAsync(Arg.Any<ShortUrl>(), Arg.Any<CancellationToken>());

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Warning)
            .Should()
            .ContainSingle();
    }

    [Fact]
    public async Task Handle_WhenCustomAliasReserved_AndValidatorBypassed_ReturnsFailureResult()
    {
        // Arrange
        var originalUrl = "https://www.example.com";
        var reservedAlias = "admin";

        _reservedAliasPolicy.IsReserved(reservedAlias).Returns(true);

        var request = new CreateShortUrlRequest(originalUrl, reservedAlias);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorType == ErrorTypes.ValidationError);

        await _shortUrlRepository.DidNotReceive()
            .AddAsync(Arg.Any<ShortUrl>(), Arg.Any<CancellationToken>());

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Warning)
            .Should()
            .ContainSingle();
    }
}
