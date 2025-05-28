using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.Filters;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Creators;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Filters;

public class TestValidateModelAttribute
{
    private readonly Mock<HttpContext> _httpContextMock;
    private readonly Mock<ActionExecutingContext> _actionExecutingContextMock;

    public TestValidateModelAttribute()
    {
        _httpContextMock = new Mock<HttpContext>();

        _actionExecutingContextMock = ActionExecutingContextMock.Create(
            TestActionContext.Create(_httpContextMock.Object));
    }

    [Fact]
    public void OnActionExecuting_WithValidArguments_ValidValidation()
    {
        // Arrange
        var firstValue = "value";
        var secondValue = 5;

        var actionArguments = new Dictionary<string, object?>
        {
            { "firstArgumentName", firstValue },
            { "secondArgumentName", secondValue },
        };
        _actionExecutingContextMock.SetupGet(c => c.ActionArguments)
            .Returns(actionArguments);

        var validationResultMock = new Mock<ValidationResult>();
        validationResultMock.SetupGet(r => r.IsValid)
            .Returns(true);

        var firstModelValidatorMock = new Mock<IValidator<string>>();
        firstModelValidatorMock.Setup(v => v.Validate(firstValue))
            .Returns(validationResultMock.Object);

        var secondModelValidatorMock = new Mock<IValidator<int>>();
        secondModelValidatorMock.Setup(v => v.Validate(secondValue))
            .Returns(validationResultMock.Object);

        _httpContextMock.Setup(c => c.RequestServices.GetService(typeof(IValidator<string>)))
            .Returns(firstModelValidatorMock.Object);

        _httpContextMock.Setup(c => c.RequestServices.GetService(typeof(IValidator<int>)))
            .Returns(secondModelValidatorMock.Object);

        var validateModelFilter = new ValidateModelAttribute();

        // Act
        validateModelFilter.OnActionExecuting(_actionExecutingContextMock.Object);

        // Assert
        _actionExecutingContextMock.VerifyGet(
            c => c.ActionArguments,
            Times.Once());

        _httpContextMock.Verify(
            c => c.RequestServices.GetService(It.IsAny<Type>()),
            Times.Exactly(2));

        validationResultMock.VerifyGet(
            r => r.IsValid,
            Times.Exactly(2));

        firstModelValidatorMock.Verify(
            v => v.Validate(It.IsAny<string>()),
            Times.Once());

        secondModelValidatorMock.Verify(
            v => v.Validate(It.IsAny<int>()),
            Times.Once());

        _actionExecutingContextMock.VerifySet(
            c => c.Result = It.IsAny<IActionResult>(),
            Times.Never());
    }

    [Fact]
    public void OnActionExecuting_WithInvalidArgument_ReturnBadRequestResponse()
    {
        // Arrange
        var modelValue = "value";
        var firstErrorMessage = "First error message.";
        var secondErrorMessage = "Second error message.";

        var actionArguments = new Dictionary<string, object?>
        {
            { "ArgumentName", modelValue },
        };
        _actionExecutingContextMock.SetupGet(c => c.ActionArguments)
            .Returns(actionArguments);

        var validationResultMock = new Mock<ValidationResult>();
        validationResultMock.SetupGet(r => r.IsValid)
            .Returns(false);

        var validationResult = validationResultMock.Object;
        validationResult.Errors.Add(new ValidationFailure(null, firstErrorMessage));
        validationResult.Errors.Add(new ValidationFailure(null, secondErrorMessage));

        var modelValidatorMock = new Mock<IValidator<string>>();
        modelValidatorMock.Setup(v => v.Validate(modelValue))
            .Returns(validationResultMock.Object);

        _httpContextMock.Setup(c => c.RequestServices.GetService(typeof(IValidator<string>)))
            .Returns(modelValidatorMock.Object);

        IActionResult? result = null;
        _actionExecutingContextMock.SetupSet(c => c.Result = It.IsAny<BadRequestObjectResult>())
            .Callback((IActionResult callbackResult) => result = callbackResult);

        var validateModelFilter = new ValidateModelAttribute();

        // Act
        validateModelFilter.OnActionExecuting(_actionExecutingContextMock.Object);

        // Assert
        _actionExecutingContextMock.VerifyGet(
            c => c.ActionArguments,
            Times.Once());

        _httpContextMock.Verify(
            c => c.RequestServices.GetService(It.IsAny<Type>()),
            Times.Once());

        modelValidatorMock.Verify(
            v => v.Validate(It.IsAny<string>()),
            Times.Once());

        validationResultMock.VerifyGet(
            r => r.IsValid,
            Times.Once());

        _actionExecutingContextMock.VerifySet(
            c => c.Result = It.IsAny<IActionResult>(),
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

    [Fact]
    public void OnActionExecuting_WithModelValueNull_ReturnBadRequestResponse()
    {
        // Arrange
        var argumentName = "ArgumentName";
        var actionArguments = new Dictionary<string, object?>
        {
            { argumentName, null },
        };
        _actionExecutingContextMock.SetupGet(c => c.ActionArguments)
            .Returns(actionArguments);

        IActionResult? result = null;
        _actionExecutingContextMock.SetupSet(c => c.Result = It.IsAny<BadRequestObjectResult>())
            .Callback((IActionResult callbackResult) => result = callbackResult);

        var validateModelFilter = new ValidateModelAttribute();

        // Act
        validateModelFilter.OnActionExecuting(_actionExecutingContextMock.Object);

        // Assert
        _actionExecutingContextMock.VerifySet(
            c => c.Result = It.IsAny<IActionResult>(),
            Times.Once());

        result.Should().NotBeNull()
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
    public void OnActionExecuting_WithNotRegisteredValidator_SkipValidation()
    {
        // Arrange
        var actionArguments = new Dictionary<string, object?>
        {
            { "ArgumentName", "value" },
        };
        _actionExecutingContextMock.SetupGet(c => c.ActionArguments)
            .Returns(actionArguments);

        _httpContextMock.Setup(c => c.RequestServices.GetService(typeof(IValidator<string>)))
            .Returns(null!);

        var validateModelFilter = new ValidateModelAttribute();

        // Act
        validateModelFilter.OnActionExecuting(_actionExecutingContextMock.Object);

        // Assert
        _actionExecutingContextMock.VerifyGet(
            c => c.ActionArguments,
            Times.Once());

        _httpContextMock.Verify(
            c => c.RequestServices.GetService(It.IsAny<Type>()),
            Times.Once());

        _actionExecutingContextMock.VerifySet(
            c => c.Result = It.IsAny<IActionResult>(),
            Times.Never());
    }
}
