using FluentAssertions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.FluentOperationResult;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.Models;

public class TestOperationResult
{
    [Fact]
    public void AddError_WithCorrectParameters_WorksValid()
    {
        // Arrange
        var result = new OperationResult<string>();
        var enumValue = ErrorCode.NotFound;
        var message = "Test error";

        // Act
        result.AddError(enumValue, message);

        // Assert
        result.Should().ContainSingleError(enumValue, message);
        result.Payload.Should().BeNull();
    }

    [Fact]
    public void AddUnknownError_WithCorrectParameters_WorksValid()
    {
        // Arrange
        var result = new OperationResult<string>();
        var message = "Test error";

        // Act
        result.AddUnknownError(message);

        // Assert
        result.Should().ContainSingleError(ErrorCode.UnknownError, message);
        result.Payload.Should().BeNull();
    }
}
