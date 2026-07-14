using BeTiny.Infrastructure.Services;

namespace BeTiny.UnitTests.Infrastructure.Services;

public class PasswordHasherTest
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ReturnsNonEmptyHash_WhenValidPassword()
    {
        var hash = _hasher.HashPassword("Password1");

        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashPassword_ReturnsDifferentHashes_ForSamePassword()
    {
        var password = "Password1";

        var first = _hasher.HashPassword(password);
        var second = _hasher.HashPassword(password);

        first.Should().NotBe(second);
    }

    [Fact]
    public void HashPassword_ReturnsBcryptFormatHash()
    {
        var hash = _hasher.HashPassword("Password1");

        hash.Should().StartWith("$2");
    }

    [Fact]
    public void HashPassword_ThrowsArgumentNullException_WhenPasswordNull()
    {
        Action act = () => _hasher.HashPassword(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("password");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void HashPassword_ThrowsArgumentException_WhenPasswordEmpty(string password)
    {
        Action act = () => _hasher.HashPassword(password);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("password");
    }
}
