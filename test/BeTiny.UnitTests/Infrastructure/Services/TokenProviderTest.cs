using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BeTiny.Application.Common.Options;
using BeTiny.Domain.Interfaces;
using BeTiny.Domain.ValueObjects;
using BeTiny.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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
        string nullUserId = null!;
        string email = "test@example.com";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _tokenProvider.IssueToken(nullUserId, email));
    }

    [Fact]
    public void IssueToken_EmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        string emptyUserId = "";
        string email = "test@example.com";

        // Act & Assert
        Action act = () => _tokenProvider.IssueToken(emptyUserId, email);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IssueToken_ThrowsArgumentNullException_WhenEmailIsNull()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        string nullEmail = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _tokenProvider.IssueToken(userId, nullEmail));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void IssueToken_EmptyEmail_ThrowsArgumentException(string email)
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();

        // Act & Assert
        Action act = () => _tokenProvider.IssueToken(userId, email);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IssueToken_ReturnsNonEmptyToken()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        string email = "test@example.com";

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IssueToken_TokenContainsUserIdAsSubClaim()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        string email = "test@example.com";

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);
        jwtToken.Subject.Should().Be(userId);
    }

    [Fact]
    public void IssueToken_TokenContainsEmailClaim()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        string email = "test@example.com";

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);
        jwtToken.Claims.First(c => c.Type == "email").Value.Should().Be(email);
    }

    [Fact]
    public void IssueToken_TokenContainsJtiClaim()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        string email = "test@example.com";

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);
        jwtToken.Claims.First(c => c.Type == "jti").Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IssueToken_TokenContainsIssuerAndAudience()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        string email = "test@example.com";

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);
        jwtToken.Issuer.Should().Be(_jwtOptions.Value.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtOptions.Value.Audience);
    }

    [Fact]
    public void IssueToken_ExpiresAtMatchesNowPlusExpiryMinutes()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        string email = "test@example.com";

        var now = DateTime.UtcNow;

        _dateTimeProvider.UtcNow.Returns(now);

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);
        jwtToken.ValidTo.Should().BeCloseTo(
            now.Add(_jwtOptions.Value.ExpiryMinutes),
            TimeSpan.FromSeconds(1)
        );
    }

    [Fact]
    public void IssueToken_TokenExpClaimMatchesExpiresAt()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        string email = "test@example.com";

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);
        var expClaim = jwtToken.Claims.First(c => c.Type == "exp").Value;
        var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;
        exp.Should().BeCloseTo(jwtToken.ValidTo, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void IssueToken_TokenIsSignedWithHs256()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        string email = "test@example.com";

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);
        jwtToken.Header.Alg.Should().Be(SecurityAlgorithms.HmacSha256);
    }

    [Fact]
    public void IssueToken_TokenSignatureValidatesWithSigningKey()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        string email = "test@example.com";

        var now = DateTime.UtcNow;

        _dateTimeProvider.UtcNow.Returns(now);

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Value.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.Value.SigningKey)
            ),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        Action act = () => new JwtSecurityTokenHandler().ValidateToken(
            result.Token,
            validationParameters, 
            out SecurityToken validatedToken
        );

        act.Should().NotThrow();
    }

    [Fact]
    public void IssueToken_ReturnsTokenResult_WithValidTokenAndExpiration()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        string email = "test@example.com";

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
