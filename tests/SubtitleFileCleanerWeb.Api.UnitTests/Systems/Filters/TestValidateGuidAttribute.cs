using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.Filters;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Creators;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Filters;

public class TestValidateGuidAttribute
{
    [Theory, AutoData]
    public void OnActionExecuting_WithValidGuids_ReturnNullResult
        (string firstPropName, string secondPropName, Guid validGuid)
    {
        // Arrange
        var actionArguments = new Dictionary<string, object?>
        {
            { firstPropName, validGuid },
            { "otherPropName", new object() },
            { secondPropName, Guid.Empty }
        };

        var actionExecutingContext = ActionExecutingContextCreator.Create(actionArguments: actionArguments);

        var sut = new ValidateGuidAttribute(firstPropName, secondPropName);

        // Act
        sut.OnActionExecuting(actionExecutingContext);

        // Assert
        actionExecutingContext.Result.Should().BeNull();
    }

    [Fact]
    public void OnActionExecuting_WithNonExistentPropertyName_ReturnNullResult()
    {
        // Arrange
        var actionArguments = new Dictionary<string, object?>
        {
            { "firstPropName", new object() },
            { "secondPropName", new object() }
        };

        var actionExecutingContext = ActionExecutingContextCreator.Create(actionArguments: actionArguments);

        var sut = new ValidateGuidAttribute("thirdPropName");

        // Act
        sut.OnActionExecuting(actionExecutingContext);

        // Assert
        actionExecutingContext.Result.Should().BeNull();
    }

    [Theory, AutoData]
    public void OnActionExecuting_WithInvalidGuids_ReturnBadRequestResult
        (string firstPropName, string secondPropName)
    {
        // Arrange
        var actionArguments = new Dictionary<string, object?>
        {
            { firstPropName, "FooName" },
            { "otherPropName", new object() },
            { secondPropName, "299261d5-b0da-41d3-92eb-c2a213538f5" }
        };

        var actionExecutingContext = ActionExecutingContextCreator.Create(actionArguments: actionArguments);

        var sut = new ValidateGuidAttribute(firstPropName, secondPropName);

        // Act
        sut.OnActionExecuting(actionExecutingContext);

        // Assert
        actionExecutingContext.Result.Should().NotBeNull()
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
