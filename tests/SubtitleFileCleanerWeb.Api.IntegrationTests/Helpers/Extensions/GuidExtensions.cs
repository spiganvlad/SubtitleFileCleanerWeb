namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Extensions;

public static class GuidExtensions
{
    public static string ToInvalidString(this Guid guid) =>
        guid.ToString()[..^1] + 'W';
}
