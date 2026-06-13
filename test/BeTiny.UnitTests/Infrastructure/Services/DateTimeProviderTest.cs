using BeTiny.Domain.Interfaces;
using BeTiny.Infrastructure.Services;

namespace BeTiny.UnitTests.Infrastructure.Services;

public sealed class DateTimeProviderTest
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public DateTimeProviderTest()
    {
        _dateTimeProvider = new DateTimeProvider();
    }

    [Fact]
    public void UtcNow_Should_Return_Current_Utc_DateTime()
    {
        // Arrange
        var expected = DateTime.UtcNow;

        // Act
        var actual = _dateTimeProvider.UtcNow;

        // Assert
        Assert.True((actual - expected).TotalSeconds < 1, "UtcNow should return the current UTC DateTime.");
    }
}
