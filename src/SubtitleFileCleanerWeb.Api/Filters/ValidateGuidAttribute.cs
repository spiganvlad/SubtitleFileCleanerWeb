using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SubtitleFileCleanerWeb.Api.Contracts.Common;

namespace SubtitleFileCleanerWeb.Api.Filters;

/// <summary>
/// Validates specified GUIDs.
/// </summary>
public class ValidateGuidAttribute : ActionFilterAttribute
{
    private readonly string[] _propNames;
    
    public ValidateGuidAttribute(params string[] propNames)
    {
        _propNames = propNames;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var isError = false;
        var errors = new List<string>();

        foreach (var propName in _propNames)
        {
            if (!context.ActionArguments.TryGetValue(propName, out var value))
                return;

            if (!Guid.TryParse(value?.ToString(), out _))
            {
                errors.Add($"Invalid GUID format of the parameter: {propName}.");
                isError = true;
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
