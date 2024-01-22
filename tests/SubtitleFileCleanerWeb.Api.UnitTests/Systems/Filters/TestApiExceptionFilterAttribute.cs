using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.Filters;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Creators;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Filters;

public class TestApiExceptionFilterAttribute
{
    [Fact]
    public void OnException_WithException_ReturnInternalServerErrorResponse()
    {
        // Arrange
        var exceptionContextMock = ExceptionContextMock.Create();

        var exceptionMessage = "Test unexpected error occurred.";
        exceptionContextMock.SetupGet(ec => ec.Exception.Message)
            .Returns(exceptionMessage);

        IActionResult objectResult = null!;
        exceptionContextMock.SetupSet(ec => ec.Result = It.IsAny<IActionResult>())
            .Callback((IActionResult callbackResult) => objectResult = callbackResult);

        var exceptionFilter = new ApiExceptionFilterAttribute();

        // Act
        exceptionFilter.OnException(exceptionContextMock.Object);

        // Assert
        exceptionContextMock.VerifyGet(ec => ec.Exception.Message, Times.Once());

        objectResult.Should().NotBeNull()
            .And.BeOfType<ObjectResult>()

            .Which.Should().HaveStatusCode(500)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<ErrorResponse>()

            .Which.Should().HaveStatusCode(500)
            .And.HaveStatusPhrase("Internal Server Error")
            .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Seconds())
            .And.HaveSingleError(exceptionMessage);
    }
}
