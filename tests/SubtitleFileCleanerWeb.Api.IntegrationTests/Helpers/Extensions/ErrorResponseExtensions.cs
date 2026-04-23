using AwesomeAssertions.Execution;
using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.FluentObjects;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Extensions;

public static class ErrorResponseExtensions
{
    public static ErrorResponseAssertions Should(this ErrorResponse response)
    {
        return new ErrorResponseAssertions(response, AssertionChain.GetOrCreate());
    }
}
