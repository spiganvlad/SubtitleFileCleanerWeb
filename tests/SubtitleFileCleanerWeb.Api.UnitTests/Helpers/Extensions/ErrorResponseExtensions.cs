using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;

public static class ErrorResponseExtensions
{
    public static ErrorResponseAssertion Should(this ErrorResponse response)
    {
        return new ErrorResponseAssertion(response);
    }
}
