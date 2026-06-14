using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Models;

namespace BeTiny.UnitTests.Application.Common.Models;

public class ResultTests
{
    [Fact]
    public void Success_WhenCalled_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var expectedValue = "Success";

        // Act
        var result = Result<string>.Success(expectedValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedValue);
        ((IResult)result).Value.Should().Be(expectedValue);
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void Failure_WhenCalled_ShouldCreateFailedResult()
    {
        // Arrange
        var errorType = ErrorTypes.ValidationError;
        var errorMessage = "Validation failed.";

        // Act
        var result = Result<string>.Failure(
            new Error(errorType, "PropertyName", errorMessage, ErrorSeverity.Low)
        );

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        ((IResult)result).Value.Should().BeNull();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().ContainSingle();
        result.Errors!.First().ErrorType.Should().Be(errorType);
        result.Errors!.First().PropertyName.Should().Be("PropertyName");
        result.Errors!.First().ErrorMessage.Should().Be(errorMessage);
        result.Errors!.First().Severity.Should().Be(ErrorSeverity.Low);
    }

    [Fact]
    public void Failure_WhenCalled_ShouldStoreMultipleErrors()
    {
        // Arrange
        var error1 = new Error(ErrorTypes.ValidationError, "Prop1", "Error 1", ErrorSeverity.Low);
        var error2 = new Error(ErrorTypes.ValidationError, "Prop2", "Error 2", ErrorSeverity.High);

        // Act
        var result = Result<string>.Failure(error1, error2);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.ErrorMessage == "Error 1");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Error 2");
    }

    [Fact]
    public void Failure_WhenErrorsNull_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => Result<string>.Failure(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Failure_WhenErrorsEmpty_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => Result<string>.Failure(Array.Empty<Error>());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Failure_WhenErrorsContainNull_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => Result<string>.Failure([ null! ]);
        act.Should().Throw<ArgumentException>();
    }
}
