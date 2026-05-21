using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Models;

namespace BeTiny.Tests.Application.Common.Models;

public class ResultTest
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var expectedValue = "Success";

        // Act
        var result = Result<string>.Success(expectedValue);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var errorType = ErrorTypes.ValidationError;
        var errorMessage = "Validation failed.";

        // Act
        var result = Result<string>.Failure(
            new Error(errorType, "PropertyName", errorMessage, ErrorSeverity.Low)
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors!);
        Assert.Equal(errorType, result.Errors!.First().ErrorType);
        Assert.Equal(errorMessage, result.Errors!.First().ErrorMessage);
    }

    [Fact]
    public void Failure_ThrowsWhenErrorsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Result<string>.Failure(null!));
    }

    [Fact]
    public void Failure_ThrowsWhenErrorsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Result<string>.Failure(Array.Empty<Error>()));
    }

    [Fact]
    public void Failure_ThrowsWhenErrorsContainNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Result<string>.Failure([ null! ]));
    }
}
