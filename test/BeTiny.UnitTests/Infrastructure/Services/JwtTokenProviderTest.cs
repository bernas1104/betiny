using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BeTiny.Application.Common.Options;
using BeTiny.Domain.Interfaces;
using BeTiny.Domain.ValueObjects;
using BeTiny.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BeTiny.UnitTests.Infrastructure.Services;

public sealed class JwtTokenProviderTest
{
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JwtTokenProvider _tokenProvider;

    public JwtTokenProviderTest()
    {
        _jwtOptions = Substitute.For<IOptions<JwtOptions>>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();

        _jwtOptions.Value.Returns(
            new JwtOptions
            {
                Issuer = "https://betiny.io/",
                Audience = "https://betiny.io/",
                SigningKey = "j7L9EruV7pI4EG0KNNJ/zEeXae22/yL1ofC2fVmGioE=",
                ExpiryMinutes = 60
            }
        );

        _tokenProvider = new JwtTokenProvider(_jwtOptions, _dateTimeProvider);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange
        IOptions<JwtOptions> nullOptions = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JwtTokenProvider(nullOptions, _dateTimeProvider));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenDateTimeProviderIsNull()
    {
        // Arrange
        IDateTimeProvider nullDateTimeProvider = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JwtTokenProvider(_jwtOptions, nullDateTimeProvider));
    }

    [Fact]
    public void IssueToken_EmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        Guid emptyUserId = Guid.Empty;
        string email = "test@example.com";

        // Act & Assert
        Action act = () => _tokenProvider.IssueToken(emptyUserId, email);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IssueToken_ThrowsArgumentNullException_WhenEmailIsNull()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
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
        Guid userId = Guid.NewGuid();

        // Act & Assert
        Action act = () => _tokenProvider.IssueToken(userId, email);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IssueToken_ReturnsNonEmptyToken()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
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
        Guid userId = Guid.NewGuid();
        string email = "test@example.com";

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);
        jwtToken.Subject.Should().Be(userId.ToString());
    }

    [Fact]
    public void IssueToken_TokenContainsEmailClaim()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
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
        Guid userId = Guid.NewGuid();
        string email = "test@example.com";

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);
        jwtToken.Claims.First(c => c.Type == "jti").Value.Should().NotBeNullOrEmpty();
        Guid.TryParse(jwtToken.Claims.First(c => c.Type == "jti").Value, out var g).Should().BeTrue();
        g.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void IssueToken_TokenContainsIssuerAndAudience()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
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
        Guid userId = Guid.NewGuid();
        string email = "test@example.com";

        var now = DateTime.UtcNow;

        _dateTimeProvider.UtcNow.Returns(now);

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);
        jwtToken.ValidTo.Should().BeCloseTo(
            now.AddMinutes(_jwtOptions.Value.ExpiryMinutes),
            TimeSpan.FromSeconds(1)
        );
    }

    [Fact]
    public void IssueToken_TokenExpClaimMatchesExpiresAt()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string email = "test@example.com";

        var now = DateTime.UtcNow;
        _dateTimeProvider.UtcNow.Returns(now);

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);
        var expClaim = jwtToken.Claims.First(c => c.Type == "exp").Value;
        var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;
        exp.Should().BeCloseTo(result.ExpiresAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void IssueToken_TokenIsSignedWithHs256()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
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
        Guid userId = Guid.NewGuid();
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
        Guid userId = Guid.NewGuid();
        string email = "test@example.com";

        var now = DateTime.UtcNow;

        _dateTimeProvider.UtcNow.Returns(now);

        // Act
        var result = _tokenProvider.IssueToken(userId, email);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().Be(now.AddMinutes(_jwtOptions.Value.ExpiryMinutes));
    }
}
