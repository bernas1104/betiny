using BeTiny.Application.Common.Options;
using BeTiny.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace BeTiny.UnitTests.Infrastructure.Services;

public sealed class ReservedAliasPolicyTest
{
    private readonly ReservedAliasPolicy _reservedAliasPolicy;

    public ReservedAliasPolicyTest()
    {
        _reservedAliasPolicy = new ReservedAliasPolicy(
            new ReservedAliasDefaults(),
            Options.Create(new ReservedAliasOptions() { Aliases = ["foo", "login"]})
        );
    }

    [Fact]
    public void IsReserved_ReturnsTrue_WhenAliasInDefaults()
    {
        var isReserved = _reservedAliasPolicy.IsReserved("admin");
        isReserved.Should().BeTrue();
    }

    [Fact]
    public void IsReserved_ReturnsTrue_WhenAliasInOptionsOnly()
    {
        var isReserved = _reservedAliasPolicy.IsReserved("foo");
        isReserved.Should().BeTrue();
    }

    [Fact]
    public void IsReserved_ReturnsTrue_WhenAliasInDefaultsAndOptionsDuplicated()
    {
        var isReserved = _reservedAliasPolicy.IsReserved("login");
        isReserved.Should().BeTrue();
    }

    [Fact]
    public void IsReserved_ReturnsFalse_WhenAliasNotReserved()
    {
        var isReserved = _reservedAliasPolicy.IsReserved("notreserved");
        isReserved.Should().BeFalse();
    }

    [Fact]
    public void IsReserved_IsCaseInsensitive()
    {
        var isReserved = _reservedAliasPolicy.IsReserved("AdMiN");
        isReserved.Should().BeTrue();
    }

    [Fact]
    public void IsReserved_ReturnsFalse_WhenAliasEmpty()
    {
        var isReserved = _reservedAliasPolicy.IsReserved("");
        isReserved.Should().BeFalse();
    }

    [Fact]
    public void Constructor_DoesNotThrow_WhenAllOptionsEntriesValid()
    {
        var act = () => new ReservedAliasPolicy(
            new ReservedAliasDefaults(),
            Options.Create(new ReservedAliasOptions() { Aliases = ["foo", "bar-baz"] })
        );

        act.Should().NotThrow();
    }

    public static IEnumerable<object[]> InvalidReservedAliasOptions => [
        ["v1"],
        [""],
        ["  "],
        [new string('a', 51)]
    ];

    [Theory]
    [MemberData(nameof(InvalidReservedAliasOptions))]
    public void Constructor_ThrowsInvalidOperationException_WhenOptionsEntryInvalid(string invalidAlias)
    {
        var act = () => new ReservedAliasPolicy(
            new ReservedAliasDefaults(),
            Options.Create(new ReservedAliasOptions() { Aliases = [invalidAlias] })
        );

        act.Should().Throw<InvalidOperationException>()
            .WithInnerException<ArgumentException>();
    }
}
