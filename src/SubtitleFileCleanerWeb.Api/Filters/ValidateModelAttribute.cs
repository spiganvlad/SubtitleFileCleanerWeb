using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SubtitleFileCleanerWeb.Api.Contracts.Common;

namespace SubtitleFileCleanerWeb.Api.Filters;

/// <summary>
/// Validates DTOs via registered validators.
/// </summary>
public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var isError = false;
        var errors = new List<string>();

        foreach (var argument in context.ActionArguments)
        {
            var value = argument.Value;
            if (value == null)
            {
                isError = true;
                errors.Add($"Parameter ({argument.Key}) cannot be null.");
                continue;
            }

            var valueType = value.GetType();

            var validatorType = typeof(IValidator<>).MakeGenericType(valueType);

            var validator = context.HttpContext.RequestServices.GetService(validatorType);
            if (validator == null)
                continue;

            var validateMethod = validator.GetType().GetMethod("Validate", [valueType])!;
            var result = (ValidationResult)validateMethod.Invoke(validator, [value])!;

            if (!result.IsValid)
            {
                isError = true;
                foreach (var error in result.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
        }

        if (isError)
        {
            var response = new ErrorResponse
            {
                StatusCode = 400,
                StatusPhrase = "Bad Request",
                Timestamp = DateTime.UtcNow,
                Errors = errors
            };

            context.Result = new BadRequestObjectResult(response);
        }
    }
}
