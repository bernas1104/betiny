using BeTiny.Domain.ValueObjects;

namespace BeTiny.UnitTests.Domain.ValueObjects;

public class EmailTest
{
    [Fact]
    public void Create_NormalizesToLowercaseAndTrims()
    {
        var email = Email.Create("  Foo@Example.COM  ");

        email.Value.Should().Be("foo@example.com");
    }

    [Fact]
    public void Create_ThrowsArgumentNullException_WhenNull()
    {
        Action act = () => Email.Create(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ThrowsArgumentException_WhenEmptyOrWhitespace(string input)
    {
        Action act = () => Email.Create(input);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("email");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("a@b")]
    [InlineData("@x.com")]
    [InlineData("a@.com")]
    [InlineData("a b@c.com")]
    public void Create_ThrowsArgumentException_WhenInvalidFormat(string input)
    {
        Action act = () => Email.Create(input);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid email format.*")
            .WithParameterName("email");
    }

    [Fact]
    public void Create_ThrowsArgumentException_WhenLongerThan254()
    {
        var longEmail = new string('a', 250) + "@x.com";

        Action act = () => Email.Create(longEmail);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("email");
    }

    [Fact]
    public void Create_Succeeds_ForValidEmail()
    {
        var email = Email.Create("user@example.com");

        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Equality_IsCaseInsensitiveBecauseNormalized()
    {
        var first = Email.Create("A@X.com");
        var second = Email.Create("a@x.com");

        first.Should().Be(second);
    }

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("A@X.COM", true)]
    [InlineData("notanemail", false)]
    [InlineData("a@b", false)]
    [InlineData("@x.com", false)]
    [InlineData("a b@c.com", false)]
    public void EmailRegex_MatchesValid_RejectsInvalid(string input, bool expected)
    {
        var normalized = input.Trim().ToLowerInvariant();
        var isMatch = Email.EmailRegex().IsMatch(normalized);

        isMatch.Should().Be(expected);
    }

    [Theory]
    [InlineData("user@example.com", "u***@example.com")]
    [InlineData("a@x.com", "a***@x.com")]
    [InlineData("john.doe@sub.domain.co.uk", "j***@sub.domain.co.uk")]
    public void RedactedValue_MasksLocalPart(string input, string expected)
    {
        var email = Email.Create(input);

        email.RedactedValue.Should().Be(expected);
    }
}
