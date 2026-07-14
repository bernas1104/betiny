using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Entities;
using BeTiny.Domain.Enums;
using BeTiny.Domain.ValueObjects;

namespace BeTiny.UnitTests.Domain.Entities;

public class UserTest
{
    private readonly IPasswordHasher _passwordHasher;

    public UserTest()
    {
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("$2a$12$mockhash");
    }

    [Fact]
    public void Create_SetsEmailAndPasswordHashAndPlan_WhenValidInputs()
    {
        var email = Email.Create("user@example.com");
        var password = "Password1";

        var user = User.Create(email, password, _passwordHasher);

        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be("$2a$12$mockhash");
        user.Plan.Should().Be(Plans.Free);
    }

    [Fact]
    public void Create_HashesPassword_NotStoredAsPlaintext()
    {
        var email = Email.Create("user@example.com");
        var password = "Password1";

        var user = User.Create(email, password, _passwordHasher);

        user.PasswordHash.Should().NotBe(password);
        user.PasswordHash.Should().StartWith("$2");
        _passwordHasher.Received(1).HashPassword(password);
    }

    [Fact]
    public void Create_ThrowsArgumentNullException_WhenEmailNull()
    {
        Action act = () => User.Create(null!, "Password1", _passwordHasher);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("email");
    }

    [Fact]
    public void Create_ThrowsArgumentNullException_WhenHasherNull()
    {
        var email = Email.Create("user@example.com");

        Action act = () => User.Create(email, "Password1", null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("passwordHasher");
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenPasswordNull()
    {
        var email = Email.Create("user@example.com");

        Action act = () => User.Create(email, null!, _passwordHasher);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("password");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ThrowsArgumentException_WhenPasswordEmpty(string password)
    {
        var email = Email.Create("user@example.com");

        Action act = () => User.Create(email, password, _passwordHasher);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("password");
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenPasswordTooShort()
    {
        var email = Email.Create("user@example.com");

        Action act = () => User.Create(email, "Pass1", _passwordHasher);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("password");
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenPasswordTooLong()
    {
        var email = Email.Create("user@example.com");
        var longPassword = "A" + new string('a', 71) + "1";

        Action act = () => User.Create(email, longPassword, _passwordHasher);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("password");
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenPasswordMissingUppercase()
    {
        var email = Email.Create("user@example.com");

        Action act = () => User.Create(email, "abcdefg1", _passwordHasher);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("password");
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenPasswordMissingLowercase()
    {
        var email = Email.Create("user@example.com");

        Action act = () => User.Create(email, "ABCDEFG1", _passwordHasher);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("password");
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenPasswordMissingDigit()
    {
        var email = Email.Create("user@example.com");

        Action act = () => User.Create(email, "Abcdefgh", _passwordHasher);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("password");
    }

    [Fact]
    public void Create_SetsPlanToFree()
    {
        var email = Email.Create("user@example.com");

        var user = User.Create(email, "Password1", _passwordHasher);

        user.Plan.Should().Be(Plans.Free);
    }

    [Fact]
    public void Create_SetsIsActiveTrue()
    {
        var email = Email.Create("user@example.com");

        var user = User.Create(email, "Password1", _passwordHasher);

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_SetsCreatedAtToRecentUtcNow()
    {
        var before = DateTime.UtcNow;
        var email = Email.Create("user@example.com");

        var user = User.Create(email, "Password1", _passwordHasher);

        var after = DateTime.UtcNow;
        user.CreatedAt.Should().BeOnOrAfter(before);
        user.CreatedAt.Should().BeOnOrBefore(after);
    }
}
