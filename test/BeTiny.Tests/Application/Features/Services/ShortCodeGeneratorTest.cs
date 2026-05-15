using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Features.Services;
using Moq;

namespace BeTiny.Tests.Application.Features.Services;

public class ShortCodeGeneratorTest
{
    private readonly Mock<IKVStore> kVStoreMock;
    private readonly ShortCodeGenerator shortCodeGenerator;

    private const long MaxHashSeed = 3521614606207L; // 62^7 - 1

    public ShortCodeGeneratorTest()
    {
        kVStoreMock = new Mock<IKVStore>();
        shortCodeGenerator = new ShortCodeGenerator(kVStoreMock.Object);
    }

    [Fact]
    public async Task GenerateShortCode_ThrowsWhenCancellationRequested()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => shortCodeGenerator.GenerateShortCode(cts.Token)
        );
    }

    [Fact]
    public async Task GenerateShortCode_ThrowsWhenHashSeedIsNegative()
    {
        kVStoreMock.Setup(kv => kv.GetNextHashSeed(It.IsAny<CancellationToken>()))
            .ReturnsAsync(-1);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => shortCodeGenerator.GenerateShortCode()
        );
    }

    [Fact]
    public async Task GenerateShortCode_ReturnsExpectedShortCode()
    {
        kVStoreMock.Setup(kv => kv.GetNextHashSeed(It.IsAny<CancellationToken>()))
            .ReturnsAsync(12345);

        var result = await shortCodeGenerator.GenerateShortCode();

        Assert.Equal("3D7", result);
    }

    [Fact]
    public async Task GenerateShortCode_ThrowsWhenSeedIsGreaterThanMaxValue()
    {
        kVStoreMock.Setup(kv => kv.GetNextHashSeed(It.IsAny<CancellationToken>()))
            .ReturnsAsync(MaxHashSeed + 1); // 62^7

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => shortCodeGenerator.GenerateShortCode()
        );
    }
}
