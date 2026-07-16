using BeTiny.Infrastructure.Services;

namespace BeTiny.UnitTests.Infrastructure.Services;

public class PasswordHasherTest
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ValidPassword_ReturnsNonEmptyHash()
    {
        var hash = _hasher.HashPassword("Password1");

        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashPassword_SamePassword_ReturnsDifferentHashes()
    {
        var password = "Password1";

        var first = _hasher.HashPassword(password);
        var second = _hasher.HashPassword(password);

        first.Should().NotBe(second);
    }

    [Fact]
    public void HashPassword_ValidPassword_ReturnsBcryptFormatHash()
    {
        var hash = _hasher.HashPassword("Password1");

        hash.Should().StartWith("$2");
    }

    [Fact]
    public void HashPassword_NullPassword_ThrowsArgumentNullException()
    {
        Action act = () => _hasher.HashPassword(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("password");
    }

    [Theory]
    [InlineData("Pässw0rd\u00e9")]     // accented (2-byte UTF-8)
    [InlineData("パスワード1aA")]       // CJK (3-byte UTF-8)
    [InlineData("P@ss1\U0001F600")]    // emoji (4-byte UTF-8)
    public void HashPassword_HandlesMultibytePassword_WithoutTruncation(string password)
    {
        var hash = _hasher.HashPassword(password);

        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("$2");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void HashPassword_EmptyPassword_ThrowsArgumentException(string password)
    {
        Action act = () => _hasher.HashPassword(password);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("password");
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var password = "Password1";
        var hash = _hasher.HashPassword(password);

        var result = _hasher.VerifyPassword(password, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var password = "Password1";
        var hash = _hasher.HashPassword(password);

        var result = _hasher.VerifyPassword("WrongPassword", hash);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Pässw0rd\u00e9")]    // accented (2-byte UTF-8)
    [InlineData("パスワード1aA")]       // CJK (3-byte UTF-8)
    [InlineData("P@ss1\U0001F600")]   // emoji (4-byte UTF-8)
    public void VerifyPassword_MultibytePassword_ReturnsTrue(string password)
    {
        var hash = _hasher.HashPassword(password);

        var result = _hasher.VerifyPassword(password, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_NullPassword_ThrowsArgumentNullException()
    {
        Action act = () => _hasher.VerifyPassword(null!, "someHash");

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("password");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyPassword_EmptyPassword_ThrowsArgumentException(string password)
    {
        Action act = () => _hasher.VerifyPassword(password, "someHash");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("password");
    }

    [Fact]
    public void VerifyPassword_NullHash_ThrowsArgumentNullException()
    {
        Action act = () => _hasher.VerifyPassword("somePassword", null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("hashedPassword");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyPassword_EmptyHash_ThrowsArgumentException(string hash)
    {
        Action act = () => _hasher.VerifyPassword("somePassword", hash);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("hashedPassword");
    }

    [Theory]
    [InlineData("not-a-hash")]
    [InlineData("garbage$$$")]
    [InlineData("$2a$invalid")]
    public void VerifyPassword_MalformedHash_ReturnsFalse(string hash)
    {
        var result = _hasher.VerifyPassword("somePassword", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void DummyPasswordHash_IsNonEmptyAndBcryptFormat()
    {
        var dummyHash = _hasher.DummyPasswordHash;

        dummyHash.Should().NotBeNullOrEmpty();
        dummyHash.Should().StartWith("$2");
    }

    [Fact]
    public void VerifyPassword_AnyPasswordAgainstDummyHash_ReturnsFalse()
    {
        var dummyHash = _hasher.DummyPasswordHash;

        var result = _hasher.VerifyPassword("anyPassword", dummyHash);

        result.Should().BeFalse();
    }
}
