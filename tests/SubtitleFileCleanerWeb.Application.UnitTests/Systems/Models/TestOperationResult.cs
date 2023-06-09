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
        var result = new OperationResult<string>();
        var enumValue = ErrorCode.NotFound;
        var message = "Test error";

        // Act
        result.AddError(enumValue, message);

        // Assert
        result.Payload.Should().BeNull();
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Code.Should().Be(enumValue);
        result.Errors[0].Message.Should().Be(message);
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
        result.Payload.Should().BeNull();
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Code.Should().Be(ErrorCode.UnknownError);
        result.Errors[0].Message.Should().Be(message);
    }
}
