using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;

namespace BeTiny.UnitTests.Application.Features.UrlShortening.Validators;

public class CreateShortUrlRequestValidatorTest
{
    private readonly CreateShortUrlRequestValidator _validator = new();

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://www.example.com")]
    public void GivenValidUrl_WhenValidated_ThenNoErrors(string url)
    {
        var request = new CreateShortUrlRequest(url);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GivenInvalidUrl_WhenValidated_ThenErrors()
    {
        var request = new CreateShortUrlRequest("invalid-url");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateShortUrlRequest.OriginalUrl));
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("ftps://example.com")]
    [InlineData("gopher://example.com")]
    [InlineData("ws://example.com")]
    [InlineData("wss://example.com")]
    [InlineData("mailto:example@example.com")]
    [InlineData("file:///path/to/file")]
    public void GivenUnsupportedUrlScheme_WhenValidated_ThenErrors(string url)
    {
        var request = new CreateShortUrlRequest(url);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateShortUrlRequest.OriginalUrl));
    }

    public static IEnumerable<object[]> InvalidCustomAliasData => new[]
    {
        ["ab"],
        ["a"],
        ["a".PadLeft(51, 'a')],
        new object[] { "invalid alias!" }
    };

    [Theory]
    [MemberData(nameof(InvalidCustomAliasData))]
    public void GivenInvalidCustomAlias_WhenValidated_ThenErrors(string customAlias)
    {
        var request = new CreateShortUrlRequest("http://example.com", customAlias);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateShortUrlRequest.CustomAlias));
    }

    [Fact]
    public void GivenValidCustomAlias_WhenValidated_ThenNoErrors()
    {
        var request = new CreateShortUrlRequest("http://example.com", "valid_alias-123");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GivenExpiresAtInLocalTime_WhenValidated_ThenErrors()
    {
        var localTime = DateTime.Now.AddDays(1);
        var request = new CreateShortUrlRequest("http://example.com", ExpiresAt: localTime);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.PropertyName == nameof(CreateShortUrlRequest.ExpiresAt));
    }

    [Fact]
    public void GivenExpiresAtInThePast_WhenValidated_ThenErrors()
    {
        var pastTime = DateTime.UtcNow.AddDays(-1);
        var request = new CreateShortUrlRequest("http://example.com", ExpiresAt: pastTime);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.PropertyName == nameof(CreateShortUrlRequest.ExpiresAt));
    }
}
