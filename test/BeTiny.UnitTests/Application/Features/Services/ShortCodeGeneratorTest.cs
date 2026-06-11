using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Features.Services;

namespace BeTiny.UnitTests.Application.Features.Services;

public class ShortCodeGeneratorTest
{
    private readonly IKVStore _kVStore;
    private readonly ShortCodeGenerator _shortCodeGenerator;

    private const long MaxHashSeed = 3521614606207L; // 62^7 - 1

    public ShortCodeGeneratorTest()
    {
        _kVStore = Substitute.For<IKVStore>();
        _shortCodeGenerator = new ShortCodeGenerator(_kVStore);
    }

    [Fact]
    public async Task GenerateShortCode_ThrowsWhenCancellationRequested()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await _shortCodeGenerator.Invoking(x => x.GenerateShortCode(cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GenerateShortCode_ThrowsWhenHashSeedIsNegative()
    {
        _kVStore.GetNextHashSeed()
            .Returns(-1);

        await _shortCodeGenerator.Invoking(x => x.GenerateShortCode())
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GenerateShortCode_ReturnsExpectedShortCode()
    {
        _kVStore.GetNextHashSeed()
            .Returns(12345);

        var result = await _shortCodeGenerator.GenerateShortCode();

        result.Should().Be("3D7");
    }

    [Fact]
    public async Task GenerateShortCode_ThrowsWhenSeedIsGreaterThanMaxValue()
    {
        _kVStore.GetNextHashSeed()
            .Returns(MaxHashSeed + 1); // 62^7

        await _shortCodeGenerator.Invoking(x => x.GenerateShortCode())
            .Should().ThrowAsync<InvalidOperationException>();
    }
}
