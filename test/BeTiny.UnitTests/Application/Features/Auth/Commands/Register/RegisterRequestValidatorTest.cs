using BeTiny.Application.Features.Auth.Commands.Register;

namespace BeTiny.UnitTests.Application.Features.Auth.Commands.Register;

public class RegisterRequestValidatorTest
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void GivenValidEmailAndPassword_WhenValidated_ThenNoErrors()
    {
        var request = new RegisterRequest("user@example.com", "Password1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GivenWhitespacePaddedValidEmail_WhenValidated_ThenNoErrors()
    {
        var request = new RegisterRequest("  user@example.com  ", "Password1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenEmptyEmail_WhenValidated_ThenErrorsWithPropertyEmail(string? email)
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
    public void GivenInvalidEmailFormat_WhenValidated_ThenErrors(string email)
    {
        var request = new RegisterRequest(email, "Password1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    [Fact]
    public void GivenEmailTooLong_WhenValidated_ThenErrors()
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
    public void GivenEmptyPassword_WhenValidated_ThenErrorsWithPropertyPassword(string? password)
    {
        var request = new RegisterRequest("user@example.com", password!);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void GivenPasswordTooShort_WhenValidated_ThenErrors()
    {
        var request = new RegisterRequest("user@example.com", "Pass1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void GivenPasswordTooLong_WhenValidated_ThenErrors()
    {
        var longPassword = "A" + new string('a', 71) + "1";
        var request = new RegisterRequest("user@example.com", longPassword);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void GivenPasswordMissingUppercase_WhenValidated_ThenErrors()
    {
        var request = new RegisterRequest("user@example.com", "abcdefg1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void GivenPasswordMissingLowercase_WhenValidated_ThenErrors()
    {
        var request = new RegisterRequest("user@example.com", "ABCDEFG1");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void GivenPasswordMissingDigit_WhenValidated_ThenErrors()
    {
        var request = new RegisterRequest("user@example.com", "Abcdefgh");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }
}
