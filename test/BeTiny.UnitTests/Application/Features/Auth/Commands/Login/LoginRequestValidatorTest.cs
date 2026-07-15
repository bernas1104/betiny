using BeTiny.Application.Features.Auth.Commands.Login;

namespace BeTiny.UnitTests.Application.Features.Auth.Commands.Login;

public sealed class LoginRequestValidatorTest
{
    private readonly LoginRequestValidator _validator = new ();

    [Fact]
    public void Validate_ValidEmailAndPassword_ReturnsSuccess()
    {
        var request = new LoginRequest("test@example.com", "Password123");
        
        var result = _validator.Validate(request);

        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhitespacePaddedEmail_ReturnsSuccess()
    {
        var request = new LoginRequest("  test@example.com  ", "Password123");
        
        var result = _validator.Validate(request);
        
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }

    [Theory]
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
    [InlineData(null)]
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyEmail_ReturnsErrorForEmail(string email)
    {
        var request = new LoginRequest(email, "Password123");
        
        var result = _validator.Validate(request);
        
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("a@b")]
    [InlineData("@x.com")]
    public void Validate_InvalidEmailFormat_ReturnsErrorForEmail(string email)
    {
        var request = new LoginRequest(email, "Password123");
        
        var result = _validator.Validate(request);
        
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Fact]
    public void Validate_TooLongEmail_ReturnsErrorForEmail()
    {
        var longEmail = new string('a', 255) + "@example.com";
        var request = new LoginRequest(longEmail, "Password123");
        
        var result = _validator.Validate(request);
        
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Theory]
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
    [InlineData(null)]
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyPassword_ReturnsErrorForPassword(string password)
    {
        var request = new LoginRequest("test@example.com", password);
        
        var result = _validator.Validate(request);
        
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginRequest.Password));
    }

    [Theory]
    [InlineData("x")]
    [InlineData("weak")]
    [InlineData("alllowercase")]
    [InlineData("123")]
    public void Validate_PasswordThatViolatesCreationRules_ReturnsSuccess(string password)
    {
        var request = new LoginRequest("test@example.com", password);
        
        var result = _validator.Validate(request);
        
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }
}
