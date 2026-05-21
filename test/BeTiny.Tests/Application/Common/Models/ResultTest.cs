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
        Assert.Equal("PropertyName", result.Errors!.First().PropertyName);
        Assert.Equal(errorMessage, result.Errors!.First().ErrorMessage);
        Assert.Equal(ErrorSeverity.Low, result.Errors!.First().Severity);
    }

    [Fact]
    public void Failure_ShouldStoreMultipleErrors()
    {
        // Arrange
        var error1 = new Error(ErrorTypes.ValidationError, "Prop1", "Error 1", ErrorSeverity.Low);
        var error2 = new Error(ErrorTypes.ValidationError, "Prop2", "Error 2", ErrorSeverity.High);

        // Act
        var result = Result<string>.Failure(error1, error2);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors!.Count());
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Error 1");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Error 2");
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
