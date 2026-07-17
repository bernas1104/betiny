using System.Security.Claims;
using System.Security.Principal;
using BeTiny.Domain.Enums;
using BeTiny.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace BeTiny.UnitTests.Infrastructure.Services;

public sealed class CurrentUserTest
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClaimsPrincipal _claimsPrincipal;
    private readonly IIdentity _identity;
    private readonly CurrentUser _currentUser;

    public CurrentUserTest()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        
        var httpContext = Substitute.For<HttpContext>();
        _claimsPrincipal = Substitute.For<ClaimsPrincipal>();
        _identity = Substitute.For<IIdentity>();

        httpContext.User.Returns(_claimsPrincipal);
        httpContext.User.Identity.Returns(_identity);
        _httpContextAccessor.HttpContext.Returns(httpContext);
        
        _currentUser = new CurrentUser(_httpContextAccessor);
    }

    [Fact]
    public void UserId_ReturnsUserId_WhenSubClaimPresent()
    {
        var userId = Guid.NewGuid();
        
        _claimsPrincipal.FindFirst("sub").Returns(new Claim("sub", userId.ToString()));
        
        _currentUser.UserId.Should().NotBeNull();
        _currentUser.UserId.Value.Should().Be(userId);
    }

    [Fact]
    public void UserId_ReturnsNull_WhenSubClaimAbsent()
    {   
        _claimsPrincipal.FindFirst("sub").Returns((Claim?)null);
        
        _currentUser.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_ReturnsNull_WhenHttpContextNull()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        
        _currentUser.UserId.Should().BeNull();
    }

    [Fact]
    public void Email_ReturnsEmail_WhenEmailClaimPresent()
    {
        var email = "example@email.com";
        
        _claimsPrincipal.FindFirst("email").Returns(new Claim("email", email));
        
        _currentUser.Email.Should().NotBeNull();
        _currentUser.Email.Should().Be(email);
    }

    [Fact]
    public void Email_ReturnsNull_WhenEmailClaimAbsent()
    {
        _claimsPrincipal.FindFirst("email").Returns((Claim?)null);
        
        _currentUser.Email.Should().BeNull();
    }

    [Fact]
    public void Plan_ReturnsPlan_WhenPlanClaimPresent()
    {
        var plan = Plans.Pro.ToString();
        
        _claimsPrincipal.FindFirst("plan").Returns(new Claim("plan", plan));
        
        _currentUser.Plan.Should().NotBeNull();
        _currentUser.Plan.Value.Should().Be(Plans.Pro);
    }

    [Fact]
    public void Plan_ReturnsNull_WhenPlanClaimAbsent()
    {
        _claimsPrincipal.FindFirst("plan").Returns((Claim?)null);
        
        _currentUser.Plan.Should().BeNull();
    }

    [Fact]
    public void Plan_ReturnsNull_WhenPlanClaimInvalid()
    {
        _claimsPrincipal.FindFirst("plan").Returns(new Claim("plan", "invalid"));
        
        _currentUser.Plan.Should().BeNull();
    }

    [Fact]
    public void IsAuthenticated_ReturnsTrue_WhenUserAuthenticated()
    {
        _claimsPrincipal.Identity?.IsAuthenticated.Returns(true);
        
        _currentUser.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_ReturnsFalse_WhenUserNotAuthenticated()
    {
        _claimsPrincipal.Identity?.IsAuthenticated.Returns(false);
        
        _currentUser.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_ReturnsFalse_WhenHttpContextNull()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        
        _currentUser.IsAuthenticated.Should().BeFalse();
    }
}
