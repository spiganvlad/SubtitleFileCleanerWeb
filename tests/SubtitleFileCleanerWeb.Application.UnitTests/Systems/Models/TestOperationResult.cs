using FluentAssertions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.Models;

public class TestOperationResult
{
    [Fact]
    public void AddError_WithCorrectParameters_WorksValid()
    {
        // Arrange
        var result = new OperationResult<bool>();
        var enumValue = (ErrorCode)(-1);
        var message = "Test error.";

        // Act
        result.AddError(enumValue, message);

        // Assert
        result.Should().BeInErrorState()
            .And.HaveSingleError(enumValue, message)
            .And.HaveDefaultPayload();
    }

    [Fact]
    public void AddUnknownError_WithCorrectParameters_WorksValid()
    {
        // Arrange
        var result = new OperationResult<bool>();
        var message = "Test error.";

        // Act
        result.AddUnknownError(message);

        // Assert
        result.Should().BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, message)
            .And.HaveDefaultPayload();
    }

    [Fact]
    public void CopyErrors_WithErrorArray_WorksValid()
    {
        // Arrange
        var firstError = new Error 
        { 
            Code = ErrorCode.UnknownError,
            Message = "Test unknown error occurred."
        };

        var secondError = new Error
        {
            Code = ErrorCode.NotFound,
            Message = "Test not found error occurred."
        };

        var thirdError = new Error
        {
            Code = ErrorCode.UnprocessableContent,
            Message = "Test unprocessable unknown error occurred."
        };

        var errors = new Error[] { firstError, secondError, thirdError };

        var result = new OperationResult<bool>();

        // Act
        result.CopyErrors(errors);

        // Assert
        result.Should().BeInErrorState()
            .And.HaveDefaultPayload();

        result.Errors.Should().HaveCount(errors.Length)
            .And.Satisfy(
            e => e.Code == firstError.Code && e.Message == firstError.Message,
            e => e.Code == secondError.Code && e.Message == secondError.Message,
            e => e.Code == thirdError.Code && e.Message == thirdError.Message);
    }

    [Fact]
    public void CopyErrors_WithInitialError_WorksValid()
    {
        // Arrange
        var initialError = new Error
        {
            Code = ErrorCode.UnknownError,
            Message = "Test unknown initial error occurred."
        };

        var firstError = new Error
        {
            Code = ErrorCode.UnknownError,
            Message = "Test first unknown error occurred."
        };

        var secondError = new Error
        {
            Code = ErrorCode.NotFound,
            Message = "Test not found error occurred."
        };

        var errors = new List<Error> { firstError, secondError };

        var result = new OperationResult<bool>();
        result.Errors.Add(initialError);

        // Act
        result.CopyErrors(errors);

        // Assert
        result.Should().BeInErrorState()
            .And.HaveDefaultPayload();

        result.Errors.Should().HaveCount(3)
            .And.Satisfy(
            e => e.Code == initialError.Code && e.Message == initialError.Message,
            e => e.Code == firstError.Code && e.Message == firstError.Message,
            e => e.Code == secondError.Code && e.Message == secondError.Message);
    }
}
