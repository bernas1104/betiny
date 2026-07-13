using BeTiny.Application.Common.Options;
using BeTiny.Domain.Entities;

namespace BeTiny.UnitTests.Application.Common.Options;

public sealed class ReservedAliasDefaultsTest
{
    private readonly ReservedAliasDefaults _reservedAliasDefaults;

    public ReservedAliasDefaultsTest()
    {
        _reservedAliasDefaults = new ReservedAliasDefaults();
    }

    [Fact]
    public void ReservedAliasDefaults_ShouldContainValidReservedAliases()
    {
        // Act & Assert
        _reservedAliasDefaults.Aliases.Should()
            .OnlyContain(
                alias => !string.IsNullOrWhiteSpace(alias) &&
                    ShortUrl.CustomAliasRegex().IsMatch(alias)
            );
    }
}
