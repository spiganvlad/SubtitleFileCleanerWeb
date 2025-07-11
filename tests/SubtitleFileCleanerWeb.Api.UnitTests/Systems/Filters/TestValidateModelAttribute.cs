using FluentValidation;
using FluentValidation.Results;
using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.Filters;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Creators;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Filters;

public class TestValidateModelAttribute
{
    private readonly HttpContext _httpContextMock;
    private readonly ActionContext _actionContext;
    private readonly ValidateModelAttribute _sut = new();

    public TestValidateModelAttribute()
    {
        _httpContextMock = Substitute.For<HttpContext>();
        _actionContext = ActionContextCreator.Create(context: _httpContextMock);
    }

    [Theory, AutoData]
    public void OnActionExecuting_WithValidArguments_ReturnNullResult
        (string stringValue, int intValue)
    {
        // Arrange
        var actionArguments = new Dictionary<string, object?>
        {
            { "firstArgumentName", stringValue },
            { "secondArgumentName", intValue },
        };

        var actionExecutingContext = ActionExecutingContextCreator.Create(
            actionContext: _actionContext, actionArguments: actionArguments);

        var validationResultMock = Substitute.For<ValidationResult>();
        validationResultMock.IsValid.Returns(true);

        var stringModelValidatorMock = Substitute.For<IValidator<string>>();
        stringModelValidatorMock.Validate(stringValue).Returns(validationResultMock);

        var intModelValidatorMock = Substitute.For<IValidator<int>>();
        intModelValidatorMock.Validate(intValue).Returns(validationResultMock);

        _httpContextMock.RequestServices.GetService(typeof(IValidator<string>)).Returns(stringModelValidatorMock);
        _httpContextMock.RequestServices.GetService(typeof(IValidator<int>)).Returns(intModelValidatorMock);

        // Act
        _sut.OnActionExecuting(actionExecutingContext);

        // Assert
        actionExecutingContext.Result.Should().BeNull();
    }

    [Theory, AutoData]
    public void OnActionExecuting_WithInvalidArgument_ReturnBadRequestResult
        (string firstErrorMessage, string secondErrorMessage, object objectValue)
    {
        // Arrange
        var actionArguments = new Dictionary<string, object?>
        {
            { "ArgumentName", objectValue },
        };

        var actionExecutingContext = ActionExecutingContextCreator.Create(
            actionContext: _actionContext, actionArguments: actionArguments);

        var validationResultMock = Substitute.For<ValidationResult>();
        validationResultMock.IsValid.Returns(false);
        validationResultMock.Errors.Add(new ValidationFailure(null, firstErrorMessage));
        validationResultMock.Errors.Add(new ValidationFailure(null, secondErrorMessage));

        var objectValidatorMock = Substitute.For<IValidator<object>>();
        objectValidatorMock.Validate(objectValue).Returns(validationResultMock);

        _httpContextMock.RequestServices.GetService(typeof(IValidator<object>)).Returns(objectValidatorMock);

        // Act
        _sut.OnActionExecuting(actionExecutingContext);

        // Assert
        actionExecutingContext.Result.Should().NotBeNull()
            .And.BeOfType<BadRequestObjectResult>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<ErrorResponse>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
            .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Seconds())
            .And.HaveErrors(firstErrorMessage, secondErrorMessage);
    }

    [Theory, AutoData]
    public void OnActionExecuting_WithNullArgument_ReturnBadRequestResult
        (string argumentName)
    {
        // Arrange
        var actionArguments = new Dictionary<string, object?>
        {
            { argumentName, null },
        };

        var actionExecutingContext = ActionExecutingContextCreator.Create(
            actionContext: _actionContext, actionArguments: actionArguments);

        // Act
        _sut.OnActionExecuting(actionExecutingContext);

        // Assert
        actionExecutingContext.Result.Should().NotBeNull()
            .And.BeOfType<BadRequestObjectResult>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<ErrorResponse>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
            .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Seconds())
            .And.HaveErrors($"Parameter ({argumentName}) cannot be null.");
    }

    [Fact]
    public void OnActionExecuting_WithNotRegisteredValidator_ReturnNullResult()
    {
        // Arrange
        var actionArguments = new Dictionary<string, object?>
        {
            { "ArgumentName", "value" },
        };

        var actionExecutingContext = ActionExecutingContextCreator.Create(
            actionContext: _actionContext, actionArguments: actionArguments);

        // Act
        _sut.OnActionExecuting(actionExecutingContext);

        // Assert
        actionExecutingContext.Result.Should().BeNull();
    }
}
