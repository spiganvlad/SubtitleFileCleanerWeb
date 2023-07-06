namespace SubtitleFileCleanerWeb.Api;

public static class ApiRoutes
{
    public const string BaseRoute = "api/[controller]";

    public static class Common
    {
        public const string GuidIdRoute = "{guidId}";
    }

    public static class FileContext
    {
        public const string ConversionType = "{conversionType}";
    }
}
