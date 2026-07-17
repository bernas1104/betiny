using BeTiny.Application.Features.Auth.Queries.GetMe;
using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Enums;
using BeTiny.Domain.ValueObjects;
using Bogus;

namespace BeTiny.UnitTests.Application.Features.Auth.Queries.GetMe;

public sealed class GetMeQueryTest
{
    private readonly ICurrentUser _currentUser;
    private readonly GetMeQuery _getMeQuery;

    public GetMeQueryTest()
    {
        _currentUser = Substitute.For<ICurrentUser>();
        _getMeQuery = new GetMeQuery(_currentUser);
    }

    [Fact]
    public async Task Handle_ReturnsGetMeResponse_WhenCurrentUserAuthenticated()
    {
        var userId = UserId.CreateUnique();
        var email = "test@example.com";
        var plan = new Faker().PickRandom<Plans>();

        _currentUser.UserId.Returns(userId);
        _currentUser.Email.Returns(email);
        _currentUser.Plan.Returns(plan);
        _currentUser.IsAuthenticated.Returns(true);

        var response = await _getMeQuery.Handle(new GetMeRequest(), CancellationToken.None);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();

        response.Value.Should().NotBeNull();
        response.Value.Id.Should().Be(userId.Value);
        response.Value.Email.Should().Be(email);
        response.Value.Plan.Should().Be(plan.ToString());
    }

    [Fact]
    public async Task Handle_ThrowsInvalidOperationException_WhenCurrentUserUserIdIsNull()
    {
        _currentUser.UserId.Returns((UserId?)null);

        Func<Task> act = async () => await _getMeQuery.Handle(new GetMeRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
