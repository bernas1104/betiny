using BeTiny.Application.Common.Options;
using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Entities;
using BeTiny.Domain.Exceptions;
using BeTiny.Domain.Interfaces;
using BeTiny.Domain.ValueObjects;
using Bogus;

namespace BeTiny.UnitTests.Domain.Entities;

public sealed class ShortUrlTest
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IReservedAliasPolicy _reservedAliasPolicy;
    private readonly Faker _faker = new ();

    public ShortUrlTest()
    {
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _reservedAliasPolicy = Substitute.For<IReservedAliasPolicy>();
    }

    [Fact]
    public void SetExpiration_SetsExpiresAt_WhenValidDateTime()
    {
        var shortUrl = ShortUrl.CreateFromShortCode(
            "https://example.com",
            "abc123",
            null,
            _dateTimeProvider
        );

        var baseUtc = DateTime.UtcNow;
        var futureDate = baseUtc.AddDays(1);
        _dateTimeProvider.UtcNow.Returns(baseUtc);

        shortUrl.SetExpiration(futureDate, _dateTimeProvider);
        var isExpired = shortUrl.IsExpired(_dateTimeProvider);

        isExpired.Should().BeFalse();
        shortUrl.ExpiresAt.Should().NotBeNull();
        shortUrl.ExpiresAt.Should().Be(futureDate);
    }

    [Fact]
    public void SetExpiration_ExpiresShortUrl_WhenExpirationDateIsInThePast()
    {
        var shortUrl = ShortUrl.CreateFromShortCode(
            "https://example.com",
            "abc123",
            null,
            _dateTimeProvider
        );

        var baseUtc = DateTime.UtcNow;
        var pastDate = baseUtc.AddDays(-1);
        _dateTimeProvider.UtcNow.Returns(baseUtc.AddDays(-2), baseUtc);

        shortUrl.SetExpiration(pastDate, _dateTimeProvider);
        var isExpired = shortUrl.IsExpired(_dateTimeProvider);

        isExpired.Should().BeTrue();
        shortUrl.ExpiresAt.Should().NotBeNull();
        shortUrl.ExpiresAt.Should().Be(pastDate);
    }

    [Fact]
    public void SetExpiration_ThrowsArgumentException_WhenDateTimeIsNotUtc()
    {
        var shortUrl = ShortUrl.CreateFromShortCode(
            "https://example.com",
            "abc123",
            null,
            _dateTimeProvider
        );

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
        var shortUrl = ShortUrl.CreateFromShortCode(
            "https://example.com",
            "abc123",
            null,
            _dateTimeProvider
        );

        var baseUtc = DateTime.UtcNow;
        var pastDate = baseUtc.AddDays(-1);
        _dateTimeProvider.UtcNow.Returns(baseUtc);

        Action act = () => shortUrl.SetExpiration(pastDate, _dateTimeProvider);

        act.Should().Throw<ArgumentException>()
            .WithMessage("The ExpiresAt must be a future date and time.*")
            .WithParameterName("expiresAt");
    }

    [Fact]
    public void IsExpired_ReturnsFalse_WhenExpiresAtIsNull()
    {
        var shortUrl = ShortUrl.CreateFromShortCode(
            "https://example.com",
            "abc123",
            null,
            _dateTimeProvider
        );

        var isExpired = shortUrl.IsExpired(_dateTimeProvider);

        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ReturnsTrue_WhenExpiresAtIsInThePast()
    {
        var shortUrl = ShortUrl.CreateFromShortCode(
            "https://example.com",
            "abc123",
            null,
            _dateTimeProvider
        );

        var baseUtc = DateTime.UtcNow;
        var futureDate = baseUtc.AddDays(1);
        
        _dateTimeProvider.UtcNow
            .Returns(baseUtc, baseUtc.AddDays(2));

        shortUrl.SetExpiration(futureDate, _dateTimeProvider);
        var isExpired = shortUrl.IsExpired(_dateTimeProvider);

        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ReturnsFalse_WhenExpiresAtIsInTheFuture()
    {
        var shortUrl = ShortUrl.CreateFromShortCode(
            "https://example.com",
            "abc123",
            null,
            _dateTimeProvider
        );

        var baseUtc = DateTime.UtcNow;
        var futureDate = baseUtc.AddDays(1);
        _dateTimeProvider.UtcNow.Returns(baseUtc);

        shortUrl.SetExpiration(futureDate, _dateTimeProvider);
        var isExpired = shortUrl.IsExpired(_dateTimeProvider);

        isExpired.Should().BeFalse();
    }

    [Fact]
    public void SetAliasUrl_SetsAliasUrl_WhenValidShortCode()
    {
        var validShortCode = "abc123";

        var shortUrl = ShortUrl.CreateFromShortCode(
            "https://example.com",
            validShortCode,
            null,
            _dateTimeProvider
        );

        shortUrl.AliasUrl.Should().Be(validShortCode);
    }

    [Fact]
    public void SetAliasUrl_SetsAliasUrl_WhenCustomAliasNotReservedAndPolicyProvided()
    {
        var validCustomAlias = "custom-alias";
        
        var reservedAliasPolicy = Substitute.For<IReservedAliasPolicy>();
        reservedAliasPolicy.IsReserved(validCustomAlias).Returns(false);

        var shortUrl = ShortUrl.CreateFromCustomAlias(
            "https://example.com",
            validCustomAlias,
            null,
            _dateTimeProvider,
            reservedAliasPolicy
        );

        shortUrl.AliasUrl.Should().Be(validCustomAlias);
    }

    [Fact]
    public void SetAliasUrl_ThrowsArgumentException_WhenAliasUrlIsNullOrWhitespace()
    {
        Action act = () => ShortUrl.CreateFromShortCode(
            "https://example.com",
            "   ",
            null,
            _dateTimeProvider
        );

        act.Should().Throw<ArgumentException>()
            .WithParameterName("aliasUrl");

        act = () => ShortUrl.CreateFromShortCode(
            "https://example.com",
            null!,
            null,
            _dateTimeProvider
        );

        act.Should().Throw<ArgumentException>()
            .WithParameterName("aliasUrl");
    }

    [Fact]
    public void SetAliasUrl_ThrowsArgumentException_WhenShortCodeIsTooLong()
    {
        var longShortCode = new string('a', 11);

        Action act = () => ShortUrl.CreateFromShortCode(
            "https://example.com",
            longShortCode,
            null,
            _dateTimeProvider
        );

        act.Should().Throw<ArgumentException>()
            .WithParameterName("aliasUrl");
    }

    [Fact]
    public void SetAliasUrl_ThrowsArgumentException_WhenCustomAliasIsInvalid()
    {
        var invalidAlias = "invalid alias!";

        Action act = () => ShortUrl.CreateFromCustomAlias(
            "https://example.com",
            invalidAlias,
            null,
            _dateTimeProvider
        );

        act.Should().Throw<ArgumentException>()
            .WithParameterName("aliasUrl");
    }

    [Fact]
    public void SetAliasUrl_ThrowsArgumentException_WhenCustomAliasIsTooShort()
    {
        var invalidAlias = "in";

        Action act = () => ShortUrl.CreateFromCustomAlias(
            "https://example.com",
            invalidAlias,
            null,
            _dateTimeProvider
        );

        act.Should().Throw<ArgumentException>()
            .WithParameterName("aliasUrl");
    }

    [Fact]
    public void SetAliasUrl_ThrowsArgumentException_WhenCustomAliasIsTooLong()
    {
        var invalidAlias = new string('a', 51);

        Action act = () => ShortUrl.CreateFromCustomAlias(
            "https://example.com",
            invalidAlias,
            null,
            _dateTimeProvider
        );

        act.Should().Throw<ArgumentException>()
            .WithParameterName("aliasUrl");
    }

    public static IEnumerable<object[]> DefaultReservedAliases => 
        new ReservedAliasDefaults().Aliases.Select(alias => new object[] { alias });

    [Theory]
    [MemberData(nameof(DefaultReservedAliases))]
    public void SetAliasUrl_ThrowsReservedAliasException_WhenCustomAliasIsReservedAndPolicyProvided(
        string reservedAlias
    )
    {
        _reservedAliasPolicy.IsReserved(reservedAlias).Returns(true);

        Action act = () => ShortUrl.CreateFromCustomAlias(
            "https://example.com",
            reservedAlias,
            null,
            _dateTimeProvider,
            _reservedAliasPolicy
        );

        act.Should().Throw<ReservedAliasException>();
    }

    [Fact]
    public void SetAliasUrl_ThrowsReservedAliasException_WhenShortCodeIsReservedAndPolicyProvided()
    {
        var reservedShortCode = GenerateRandomAlias();

        _reservedAliasPolicy.IsReserved(reservedShortCode).Returns(true);

        Action act = () => ShortUrl.CreateFromShortCode(
            "https://example.com",
            reservedShortCode,
            null,
            _dateTimeProvider,
            _reservedAliasPolicy
        );

        act.Should().Throw<ReservedAliasException>();
    }

    [Fact]
    public void SetAliasUrl_SetsAliasUrl_WhenShortCodeNotReservedAndPolicyProvided()
    {
        var validShortCode = GenerateRandomAlias();

        _reservedAliasPolicy.IsReserved(validShortCode).Returns(false);

        var shortUrl = ShortUrl.CreateFromShortCode(
            "https://example.com",
            validShortCode,
            null,
            _dateTimeProvider,
            _reservedAliasPolicy
        );

        shortUrl.AliasUrl.Should().Be(validShortCode);
    }

    [Fact]
    public void SetAliasUrl_SetsAliasUrl_WhenPolicyNullAndAliasReserved()
    {
        var reservedAlias = new ReservedAliasDefaults().Aliases.First();

        var shortUrl = ShortUrl.CreateFromCustomAlias(
            "https://example.com",
            reservedAlias,
            null,
            _dateTimeProvider,
            null
        );

        shortUrl.AliasUrl.Should().Be(reservedAlias);
    }

    [Fact]
    public void SetAliasUrl_ThrowsReservedAliasException_WhenCustomAliasReservedInAnyCase()
    {
        _reservedAliasPolicy.IsReserved(
            Arg.Is<string>(alias => alias.Equals("admin", StringComparison.OrdinalIgnoreCase))
        ).Returns(true);

        Action actLowerCase = () => ShortUrl.CreateFromCustomAlias(
            "https://example.com",
            "admin",
            null,
            _dateTimeProvider,
            _reservedAliasPolicy
        );

        Action actUpperCase = () => ShortUrl.CreateFromCustomAlias(
            "https://example.com",
            "ADMIN",
            null,
            _dateTimeProvider,
            _reservedAliasPolicy
        );

        actLowerCase.Should().Throw<ReservedAliasException>();
        actUpperCase.Should().Throw<ReservedAliasException>();
    }

    [Fact]
    public void SetUserId_SetsUserId_WhenUserIdProvidedAndNotAlreadySet()
    {
        var shortUrl = ShortUrl.CreateFromShortCode(
            "https://example.com",
            "abc123",
            null,
            _dateTimeProvider
        );

        var userId = UserId.CreateUnique();

        shortUrl.SetUserId(userId);

        shortUrl.UserId.Should().Be(userId);
    }

    [Fact]
    public void SetUserId_ThrowsArgumentNullException_WhenUserIdIsNull()
    {
        var shortUrl = ShortUrl.CreateFromShortCode(
            "https://example.com",
            "abc123",
            null,
            _dateTimeProvider
        );

        Action act = () => shortUrl.SetUserId(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userId");
    }

    [Fact]
    public void SetUserId_ThrowsInvalidOperationException_WhenUserIdAlreadySet()
    {
        var shortUrl = ShortUrl.CreateFromShortCode(
            "https://example.com",
            "abc123",
            null,
            _dateTimeProvider
        );

        var userId = UserId.CreateUnique();
        shortUrl.SetUserId(userId);

        Action act = () => shortUrl.SetUserId(UserId.CreateUnique());

        act.Should().Throw<InvalidOperationException>();
    }

    private string GenerateRandomAlias() => _faker.Random.AlphaNumeric(_faker.Random.Int(3, 7));
}
