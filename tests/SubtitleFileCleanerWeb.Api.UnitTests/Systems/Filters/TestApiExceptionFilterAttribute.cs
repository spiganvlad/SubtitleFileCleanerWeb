using Microsoft.AspNetCore.Mvc.Filters;
using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.Filters;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Creators;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Filters;

public class TestApiExceptionFilterAttribute
{
    [Theory, AutoData]
    public void OnException_WithException_ReturnInternalServerErrorResult
        (string exceptionMessage)
    {
        // Arrange
        var exceptionContext = new ExceptionContext(ActionContextCreator.Create(), []) 
        { Exception = new Exception(exceptionMessage) };

        var sut = new ApiExceptionFilterAttribute();

        // Act
        sut.OnException(exceptionContext);

        // Assert
        exceptionContext.Result.Should().NotBeNull()
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
