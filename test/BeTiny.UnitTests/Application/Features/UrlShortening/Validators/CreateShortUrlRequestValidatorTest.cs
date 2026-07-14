using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;
using BeTiny.Domain.Common.Interfaces;

namespace BeTiny.UnitTests.Application.Features.UrlShortening.Validators;

public class CreateShortUrlRequestValidatorTest
{
    private readonly IReservedAliasPolicy _reservedAliasPolicy;
    private readonly CreateShortUrlRequestValidator _validator;

    public CreateShortUrlRequestValidatorTest()
    {
        _reservedAliasPolicy = Substitute.For<IReservedAliasPolicy>();
        _validator = new CreateShortUrlRequestValidator(_reservedAliasPolicy);
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://www.example.com")]
    public void Validate_ValidUrl_ReturnsSuccess(string url)
    {
        var request = new CreateShortUrlRequest(url);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidUrl_ReturnsErrorForOriginalUrl()
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
    public void Validate_UnsupportedUrlScheme_ReturnsErrorForOriginalUrl(string url)
    {
        var request = new CreateShortUrlRequest(url);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateShortUrlRequest.OriginalUrl));
    }

    public static IEnumerable<object[]> InvalidCustomAliasData => [
        ["ab"],
        ["a"],
        ["a".PadLeft(51, 'a')],
        ["invalid alias!"]
    ];

    [Theory]
    [MemberData(nameof(InvalidCustomAliasData))]
    public void Validate_InvalidCustomAlias_ReturnsErrorForCustomAlias(string customAlias)
    {
        var request = new CreateShortUrlRequest("http://example.com", customAlias);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateShortUrlRequest.CustomAlias));
    }

    [Fact]
    public void Validate_ReservedCustomAlias_ReturnsErrorForCustomAlias()
    {
        var reservedAlias = "reserved-alias";
        _reservedAliasPolicy.IsReserved(reservedAlias).Returns(true);

        var request = new CreateShortUrlRequest("http://example.com", reservedAlias);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.PropertyName == nameof(CreateShortUrlRequest.CustomAlias));
    }

    [Fact]
    public void Validate_CaseInsensitiveReservedAlias_ReturnsErrorForCustomAlias()
    {
        _reservedAliasPolicy
            .IsReserved(Arg.Is<string>(s => s.Equals("admin", StringComparison.OrdinalIgnoreCase)))
            .Returns(true);

        var request = new CreateShortUrlRequest("http://example.com", "ADMIN");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.PropertyName == nameof(CreateShortUrlRequest.CustomAlias));
    }

    [Fact]
    public void Validate_ValidCustomAlias_ReturnsSuccess()
    {
        var request = new CreateShortUrlRequest("http://example.com", "valid_alias-123");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_LocalTimeExpiresAt_ReturnsErrorForExpiresAt()
    {
        var localTime = DateTime.Now.AddDays(1);
        var request = new CreateShortUrlRequest("http://example.com", ExpiresAt: localTime);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.PropertyName == nameof(CreateShortUrlRequest.ExpiresAt));
    }

    [Fact]
    public void Validate_PastExpiresAt_ReturnsErrorForExpiresAt()
    {
        var pastTime = DateTime.UtcNow.AddDays(-1);
        var request = new CreateShortUrlRequest("http://example.com", ExpiresAt: pastTime);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.PropertyName == nameof(CreateShortUrlRequest.ExpiresAt));
    }
}
