using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SubtitleFileCleanerWeb.Api.Contracts.Common;

namespace SubtitleFileCleanerWeb.Api.Filters;

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;

        var response = new ErrorResponse
        {
            StatusCode = 400,
            StatusPhrase = "Bad Request",
            Timestamp = DateTime.UtcNow
        };

        foreach (var state in context.ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                response.Errors.Add(error.ErrorMessage);
            }
        }

        context.Result = new BadRequestObjectResult(response);
    }
}
