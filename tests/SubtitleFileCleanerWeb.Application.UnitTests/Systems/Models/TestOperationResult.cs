using AutoFixture.Xunit3;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.Models;

public class TestOperationResult
{
    private readonly OperationResult<bool> _sut = new();

    [Theory, AutoData]
    public void AddError_WithCorrectParameters_AddsError
        (ErrorCode code, string message)
    {
        // Act
        _sut.AddError(code, message);

        // Assert
        _sut.Should().BeInErrorState()
            .And.HaveSingleError(code, message)
            .And.HaveDefaultPayload();
    }

    [Theory, AutoData]
    public void AddUnknownError_WithCorrectParameters_AddsUnknownError
        (string message)
    {
        // Act
        _sut.AddUnknownError(message);

        // Assert
        _sut.Should().BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, message)
            .And.HaveDefaultPayload();
    }

    [Theory, AutoData]
    public void CopyErrors_WithErrorArray_CopyArrayToErrors
        (Error[] errors)
    {
        // Act
        _sut.CopyErrors(errors);

        // Assert
        _sut.Should().BeInErrorState()
            .And.HaveDefaultPayload();

        _sut.Errors.Should().HaveCount(errors.Length)
            .And.BeEquivalentTo(errors);
    }

    [Theory, AutoData]
    public void CopyErrors_WithInitialError_CopyArrayToErrors
        (Error initialError, Error[] errors)
    {
        // Arrange
        _sut.Errors.Add(initialError);

        // Act
        _sut.CopyErrors(errors);

        // Assert
        _sut.Should().BeInErrorState()
            .And.HaveDefaultPayload();

        _sut.Errors.Should().Contain(initialError)
            .And.Contain(errors);
    }
}
