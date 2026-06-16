using BeTiny.Domain.Entities;
using BeTiny.Domain.Enums;
using BeTiny.Domain.Interfaces;
using Bogus;

namespace BeTiny.UnitTests.Domain.Entities;

public sealed class ShortUrlTest
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly Faker _faker = new ();

    public ShortUrlTest()
    {
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    }

    [Fact]
    public void SetExpiration_SetsExpiresAt_WhenValidDateTime()
    {
        var shortUrl = new ShortUrl("https://example.com", _faker.PickRandom<AliasUrlType>());
        shortUrl.SetAliasUrl("abc123");

        var baseUtc = DateTime.UtcNow;
        var futureDate = baseUtc.AddDays(1);
        _dateTimeProvider.UtcNow.Returns(baseUtc);

        shortUrl.SetExpiration(futureDate, _dateTimeProvider);
        var isExpired = shortUrl.IsExpired();

        isExpired.Should().BeFalse();
        shortUrl.ExpiresAt.Should().NotBeNull();
        shortUrl.ExpiresAt.Should().Be(futureDate);
    }

    [Fact]
    public void SetExpiration_ExpiresShortUrl_WhenExpirationDateIsInThePast()
    {
        var shortUrl = new ShortUrl("https://example.com", _faker.PickRandom<AliasUrlType>());
        shortUrl.SetAliasUrl("abc123");

        var baseUtc = DateTime.UtcNow;
        var pastDate = baseUtc.AddDays(-1);
        _dateTimeProvider.UtcNow.Returns(baseUtc.AddDays(-2));

        shortUrl.SetExpiration(pastDate, _dateTimeProvider);
        var isExpired = shortUrl.IsExpired();

        isExpired.Should().BeTrue();
        shortUrl.ExpiresAt.Should().NotBeNull();
        shortUrl.ExpiresAt.Should().Be(pastDate);
    }

    [Fact]
    public void SetExpiration_ThrowsArgumentException_WhenDateTimeIsNotUtc()
    {
        var shortUrl = new ShortUrl("https://example.com", _faker.PickRandom<AliasUrlType>());
        shortUrl.SetAliasUrl("abc123");

        var baseUtc = DateTime.UtcNow;
        var nonUtcDate = DateTime.Now.AddDays(1);
        _dateTimeProvider.UtcNow.Returns(baseUtc);

        Action act = () => shortUrl.SetExpiration(nonUtcDate, _dateTimeProvider);

        act.Should().Throw<ArgumentException>()
            .WithMessage("The ExpiresAt must be in UTC.*")
            .WithParameterName("expiresAt");
    }

    [Fact]
    public void SetExpiration_ThrowsArgumentException_WhenDateTimeIsInThePast()
    {
        var shortUrl = new ShortUrl("https://example.com", _faker.PickRandom<AliasUrlType>());
        shortUrl.SetAliasUrl("abc123");
        
        var baseUtc = DateTime.UtcNow;
        var pastDate = baseUtc.AddDays(-1);
        _dateTimeProvider.UtcNow.Returns(baseUtc);

        Action act = () => shortUrl.SetExpiration(pastDate, _dateTimeProvider);

        act.Should().Throw<ArgumentException>()
            .WithMessage("The ExpiresAt must be a future date and time.*")
            .WithParameterName("expiresAt");
    }

    [Fact]
    public void SetAliasUrl_ThrowsArgumentException_WhenAliasUrlIsNullOrWhitespace()
    {
        var shortUrl = new ShortUrl("https://example.com", _faker.PickRandom<AliasUrlType>());

        Action act = () => shortUrl.SetAliasUrl("   ");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("aliasUrl");

        act = () => shortUrl.SetAliasUrl(null!);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("aliasUrl");
    }

    [Fact]
    public void SetAliasUrl_ThrowsArgumentException_WhenShortCodeIsTooLong()
    {
        var shortUrl = new ShortUrl("https://example.com", AliasUrlType.ShortCode);
        var longShortCode = new string('a', 8);

        Action act = () => shortUrl.SetAliasUrl(longShortCode);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("aliasUrl");
    }

    [Fact]
    public void SetAliasUrl_ThrowsArgumentException_WhenCustomAliasIsInvalid()
    {
        var shortUrl = new ShortUrl("https://example.com", AliasUrlType.CustomAlias);
        var invalidAlias = "invalid alias!";

        Action act = () => shortUrl.SetAliasUrl(invalidAlias);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("aliasUrl");
    }

    [Fact]
    public void SetAliasUrl_ThrowsArgumentException_WhenCustomAliasIsTooShort()
    {
        var shortUrl = new ShortUrl("https://example.com", AliasUrlType.CustomAlias);
        var invalidAlias = "in";

        Action act = () => shortUrl.SetAliasUrl(invalidAlias);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("aliasUrl");
    }

    [Fact]
    public void SetAliasUrl_ThrowsArgumentException_WhenCustomAliasIsTooLong()
    {
        var shortUrl = new ShortUrl("https://example.com", AliasUrlType.CustomAlias);
        var invalidAlias = new string('a', 51);

        Action act = () => shortUrl.SetAliasUrl(invalidAlias);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("aliasUrl");
    }
}
