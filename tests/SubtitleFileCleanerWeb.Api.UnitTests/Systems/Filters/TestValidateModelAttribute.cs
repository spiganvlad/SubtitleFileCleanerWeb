using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.Filters;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Creators;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Filters;

public class TestValidateModelAttribute
{
    [Fact]
    public void OnActionExecuting_WithValidModel_ReturnValid()
    {
        // Arrange
        var actionExecutingContextMock = ActionExecutingContextMock.Create();

        var validateModelFilter = new ValidateModelAttribute();

        // Act
        validateModelFilter.OnActionExecuting(actionExecutingContextMock.Object);

        // Assert
        actionExecutingContextMock.VerifySet(
            ec => ec.Result = It.IsAny<IActionResult>(),
            Times.Never());
    }

    [Fact]
    public void OnActionExecuting_WithInvalidModel_ReturnBadRequestResponse()
    {
        // Arrange
        var firstErrorMessage = "Guid id is not valid.";
        var secondErrorMessage = "ObjectA is not valid.";

        var actionExecutingContextMock = ActionExecutingContextMock.Create();

        var actionExecutingContext = actionExecutingContextMock.Object;
        actionExecutingContext.ModelState.AddModelError("guidId", firstErrorMessage);
        actionExecutingContext.ModelState.AddModelError("objectA", secondErrorMessage);

        IActionResult? result = null;
        actionExecutingContextMock.SetupSet(ec => ec.Result = It.IsAny<BadRequestObjectResult>())
            .Callback((IActionResult callbackResult) => result = callbackResult);

        var validateModelFilter = new ValidateModelAttribute();

        // Act
        validateModelFilter.OnActionExecuting(actionExecutingContext);

        // Assert
        actionExecutingContextMock.VerifySet(
            ec => ec.Result = It.IsAny<IActionResult>(),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeOfType<BadRequestObjectResult>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<ErrorResponse>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
            .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Seconds())
            .And.HaveErrors(
            firstErrorMessage,
            secondErrorMessage);
    }
}
