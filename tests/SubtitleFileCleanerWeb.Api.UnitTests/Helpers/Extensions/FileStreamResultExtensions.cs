using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;

public static class FileStreamResultExtensions
{
    public static FileStreamResultAssertion Should(this FileStreamResult result)
    {
        return new FileStreamResultAssertion(result);
    }
}
