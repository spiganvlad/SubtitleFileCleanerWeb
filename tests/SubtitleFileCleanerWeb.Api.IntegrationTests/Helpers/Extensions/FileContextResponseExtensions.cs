using AwesomeAssertions.Execution;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Responses;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.FluentObjects;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Extensions;

public static class FileContextResponseExtensions
{
    public static FileContextResponseAssertions Should(this FileContextResponse response)
    {
        return new FileContextResponseAssertions(response, AssertionChain.GetOrCreate());
    }
}
