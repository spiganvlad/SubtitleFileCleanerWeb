namespace SubtitleFileCleanerWeb.Api.Configurations
{
    public static class ConfigurationKeys
    {
        public static string FormFile { get; } = "FormFile";
        public static string DefaultCorsPolicy { get; } = "Cors:DefaultPolicy";
        public static string SQLiteConnectionString { get; } = "SQLite";
        public static string FileSystemStorage { get; } = "FileSystemStorage";
    }
}
