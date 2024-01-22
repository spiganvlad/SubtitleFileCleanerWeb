using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.Filters;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Creators;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Filters;

public class TestValidateGuidAttribute
{
    [Fact]
    public void OnActionExecuting_WithValidGuids_ReturnValid()
    {
        // Arrange
        var firstPropName = "firstGuidId";
        var secondPropName = "secondGuidId";

        var actionArguments = new Dictionary<string, object?>
        {
            { firstPropName, "299261d5-b0da-41d3-92eb-c2a213538f58" },
            { "otherPropName", new object() },
            { secondPropName, Guid.Empty }
        };

        var actionExecutingContextMock = ActionExecutingContextMock.Create();

        actionExecutingContextMock.SetupGet(ec => ec.ActionArguments)
            .Returns(actionArguments);

        actionExecutingContextMock.SetupSet(ec => ec.Result = It.IsAny<IActionResult>());

        var validateGuidFilter = new ValidateGuidAttribute(firstPropName, secondPropName);

        // Act
        validateGuidFilter.OnActionExecuting(actionExecutingContextMock.Object);

        // Assert
        actionExecutingContextMock.VerifyGet(
            ec => ec.ActionArguments,
            Times.Exactly(2));

        actionExecutingContextMock.VerifySet(
            ec => ec.Result = It.IsAny<IActionResult>(),
            Times.Never());
    }

    [Fact]
    public void OnActionExecuting_WithNonExistentPropertyName_ReturnValid()
    {
        // Arrange
        var actionArguments = new Dictionary<string, object?>
        {
            { "firstPropName", new object() },
            { "secondPropName", new object() }
        };

        var actionExecutingContextMock = ActionExecutingContextMock.Create();
        actionExecutingContextMock.SetupGet(ec => ec.ActionArguments)
            .Returns(actionArguments);

        var validateGuidFilter = new ValidateGuidAttribute("thirdPropName");

        // Act
        validateGuidFilter.OnActionExecuting(actionExecutingContextMock.Object);

        // Assert
        actionExecutingContextMock.VerifyGet(
            ec => ec.ActionArguments,
            Times.Once());

        actionExecutingContextMock.VerifySet(
            ec => ec.Result = It.IsAny<IActionResult>(),
            Times.Never());
    }

    [Fact]
    public void OnActionExecuting_WithInvalidGuids_ReturnBadRequestResponse()
    {
        // Arrange
        var firstPropName = "firstGuidId";
        var secondPropName = "secondGuidId";

        var actionArguments = new Dictionary<string, object?>
        {
            { firstPropName, "FooName" },
            { "otherPropName", new object() },
            { secondPropName, "299261d5-b0da-41d3-92eb-c2a213538f5" }
        };

        var actionExecutingContextMock = ActionExecutingContextMock.Create();

        actionExecutingContextMock.SetupGet(ec => ec.ActionArguments)
            .Returns(actionArguments);

        IActionResult? result = null;
        actionExecutingContextMock.SetupSet(ec => ec.Result = It.IsAny<BadRequestObjectResult>())
            .Callback((IActionResult callbackResult) => result = callbackResult);

        var validateGuidFilter = new ValidateGuidAttribute(firstPropName, secondPropName);

        // Act
        validateGuidFilter.OnActionExecuting(actionExecutingContextMock.Object);

        // Assert
        actionExecutingContextMock.VerifyGet(
            ec => ec.ActionArguments,
            Times.Exactly(2));

        actionExecutingContextMock.VerifySet(
            ec => ec.Result = It.IsAny<IActionResult>(),
            Times.Once());

        var x = result.Should().NotBeNull()
            .And.BeOfType<BadRequestObjectResult>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<ErrorResponse>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
            .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Seconds())
            .And.HaveErrors(
            $"Invalid GUID format of the parameter: {firstPropName}.",
            $"Invalid GUID format of the parameter: {secondPropName}.");
    }
}
