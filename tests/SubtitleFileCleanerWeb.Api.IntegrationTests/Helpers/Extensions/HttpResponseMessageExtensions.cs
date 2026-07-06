using AwesomeAssertions.Execution;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.FluentObjects;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Extensions;

public static class HttpResponseMessageExtensions
{
    public static HttpResponseMessageAssertions Should(this HttpResponseMessage response)
    {
        return new HttpResponseMessageAssertions(response, AssertionChain.GetOrCreate());
    }
}
