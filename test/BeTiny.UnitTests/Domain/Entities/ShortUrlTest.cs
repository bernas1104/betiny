using BeTiny.Domain.Entities;
using BeTiny.Domain.Interfaces;

namespace BeTiny.UnitTests.Domain.Entities;

public sealed class ShortUrlTest
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public ShortUrlTest()
    {
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    }

    [Fact]
    public void SetExpiration_SetsExpiresAt_WhenValidDateTime()
    {
        var shortUrl = new ShortUrl("https://example.com");
        shortUrl.SetShortCode("abc123");

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
        var shortUrl = new ShortUrl("https://example.com");
        shortUrl.SetShortCode("abc123");

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
        var shortUrl = new ShortUrl("https://example.com");
        shortUrl.SetShortCode("abc123");

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
        var shortUrl = new ShortUrl("https://example.com");
        shortUrl.SetShortCode("abc123");
        
        var baseUtc = DateTime.UtcNow;
        var pastDate = baseUtc.AddDays(-1);
        _dateTimeProvider.UtcNow.Returns(baseUtc);

        Action act = () => shortUrl.SetExpiration(pastDate, _dateTimeProvider);

        act.Should().Throw<ArgumentException>()
            .WithMessage("The ExpiresAt must be a future date and time.*")
            .WithParameterName("expiresAt");
    }
}
