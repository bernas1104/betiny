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
}
