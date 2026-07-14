using BeTiny.Application.Features.UrlShortening.Queries.GetByShortUrl;

namespace BeTiny.UnitTests.Application.Features.UrlShortening.Validators;

public class GetByShortUrlRequestValidatorTest
{
    private readonly GetByShortUrlRequestValidator _validator = new ();

    [Fact]
    public void Validate_ValidShortUrl_ReturnsSuccess()
    {
        var request = new GetByShortUrlRequest(
            "abcdefg",
            "UserAgent",
            "Referer",
            "IpAddress"
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
    public static IEnumerable<object[]> InvalidCustomAliasData => [
    
        [""],
        ["a".PadLeft(51, 'a')],
        ["invalid alias!"]
    ];

    [Theory]
    [MemberData(nameof(InvalidCustomAliasData))]
    public void Validate_InvalidShortUrl_ReturnsErrorForShortUrl(string shortUrl)
    {
        var request = new GetByShortUrlRequest(
            shortUrl,
            "UserAgent",
            "Referer",
            "IpAddress"
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.PropertyName == nameof(GetByShortUrlRequest.ShortUrl));
    }
}
