using BeTiny.Application.Features.Auth.Commands.Register;

namespace BeTiny.UnitTests.Application.Features.Auth.Commands.Register;

public class RegisterRequestValidatorTest
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidEmailAndPassword_ReturnsSuccess()
    {
        var request = new RegisterRequest("user@example.com", "Password1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhitespacePaddedEmail_ReturnsSuccess()
    {
        var request = new RegisterRequest("  user@example.com  ", "Password1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyEmail_ReturnsErrorForEmail(string? email)
    {
        var request = new RegisterRequest(email!, "Password1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("a@b")]
    [InlineData("@x.com")]
    public void Validate_InvalidEmailFormat_ReturnsErrorForEmail(string email)
    {
        var request = new RegisterRequest(email, "Password1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    [Fact]
    public void Validate_TooLongEmail_ReturnsErrorForEmail()
    {
        var longEmail = new string('a', 250) + "@x.com";
        var request = new RegisterRequest(longEmail, "Password1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyPassword_ReturnsErrorForPassword(string? password)
    {
        var request = new RegisterRequest("user@example.com", password!);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void Validate_ShortPassword_ReturnsErrorForPassword()
    {
        var request = new RegisterRequest("user@example.com", "Pass1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void Validate_LongPassword_ReturnsErrorForPassword()
    {
        var longPassword = "A" + new string('a', 71) + "1";
        var request = new RegisterRequest("user@example.com", longPassword);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void Validate_PasswordMissingUppercase_ReturnsErrorForPassword()
    {
        var request = new RegisterRequest("user@example.com", "abcdefg1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void Validate_PasswordMissingLowercase_ReturnsErrorForPassword()
    {
        var request = new RegisterRequest("user@example.com", "ABCDEFG1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void Validate_PasswordMissingDigit_ReturnsErrorForPassword()
    {
        var request = new RegisterRequest("user@example.com", "Abcdefgh");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }
}
