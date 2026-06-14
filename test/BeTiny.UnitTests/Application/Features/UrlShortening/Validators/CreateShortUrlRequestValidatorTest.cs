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
}
