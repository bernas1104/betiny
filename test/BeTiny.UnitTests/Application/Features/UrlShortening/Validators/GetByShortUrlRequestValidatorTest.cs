using BeTiny.Application.Features.UrlShortening.Queries.GetByShortCode;

namespace BeTiny.UnitTests.Application.Features.UrlShortening.Validators;

public class GetByShortUrlRequestValidatorTest
{
    private readonly GetByShortCodeRequestValidator _validator = new ();

    [Fact]
    public void GivenValidShortCode_WhenValidated_ThenNoErrors()
    {
        var request = new GetByShortCodeRequest(
            "abcdefg",
            "UserAgent",
            "Referer",
            "IpAddress"
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345678")]
    [InlineData("?")]
    public void GivenInvalidShortCode_WhenValidated_TheErrors(string shortCode)
    {
        var request = new GetByShortCodeRequest(
            shortCode,
            "UserAgent",
            "Referer",
            "IpAddress"
        );

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.PropertyName == nameof(GetByShortCodeRequest.ShortCode));
    }
}
