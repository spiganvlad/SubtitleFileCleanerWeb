using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SubtitleFileCleanerWeb.Api.Contracts.Common;

namespace SubtitleFileCleanerWeb.Api.Filters;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        var response = new ErrorResponse
        {
            StatusCode = 500,
            StatusPhrase = "Internal Server Error",
            Timestamp = DateTime.UtcNow
        };
        response.Errors.Add(context.Exception.Message);

        context.Result = new ObjectResult(response) { StatusCode = 500 };
    }
}
