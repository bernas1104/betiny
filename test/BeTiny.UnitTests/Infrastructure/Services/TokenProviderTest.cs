using BeTiny.Application.Common.Options;
using BeTiny.Domain.Interfaces;
using BeTiny.Domain.ValueObjects;
using BeTiny.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace BeTiny.UnitTests.Infrastructure.Services;

public sealed class TokenProviderTest
{
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly TokenProvider _tokenProvider;

    public TokenProviderTest()
    {
        _jwtOptions = Substitute.For<IOptions<JwtOptions>>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();

        _jwtOptions.Value.Returns(
            new JwtOptions
            {
                Issuer = "https://betiny.io/",
                Audience = "https://betiny.io/",
                SigningKey = "j7L9EruV7pI4EG0KNNJ/zEeXae22/yL1ofC2fVmGioE=",
                ExpiryMinutes = TimeSpan.FromMinutes(60)
            }
        );

        _tokenProvider = new TokenProvider(_jwtOptions, _dateTimeProvider);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange
        IOptions<JwtOptions> nullOptions = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TokenProvider(nullOptions, _dateTimeProvider));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenDateTimeProviderIsNull()
    {
        // Arrange
        IDateTimeProvider nullDateTimeProvider = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TokenProvider(_jwtOptions, nullDateTimeProvider));
    }

    [Fact]
    public void IssueToken_ThrowsArgumentNullException_WhenUserIdIsNull()
    {
        // Arrange
        UserId nullUserId = null!;
        var email = Email.Create("test@example.com");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _tokenProvider.IssueToken(nullUserId, email));
    }

    [Fact]
    public void IssueToken_ThrowsArgumentNullException_WhenEmailIsNull()
    {
        // Arrange
        var userId = UserId.Create(Guid.NewGuid());
        Email nullEmail = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _tokenProvider.IssueToken(userId, nullEmail));
    }

    [Fact]
    public void IssueToken_ReturnsTokenResult_WithValidTokenAndExpiration()
    {
        // Arrange
        var userId = UserId.Create(Guid.NewGuid());
        var email = Email.Create("test@example.com");

        var now = DateTime.UtcNow;

        _dateTimeProvider.UtcNow.Returns(now);

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().Be(now.Add(_jwtOptions.Value.ExpiryMinutes));
    }
}
