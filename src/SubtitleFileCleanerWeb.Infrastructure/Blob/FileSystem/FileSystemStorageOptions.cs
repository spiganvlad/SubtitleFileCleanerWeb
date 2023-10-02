using System.Reflection;

namespace SubtitleFileCleanerWeb.Infrastructure.Blob.FileSystem;

public class FileSystemStorageOptions
{
    public string RelativePath { get; set; } = null!;

    public string GetFullPath()
    {
        var applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                ?? throw new InvalidOperationException("Unable to determine the executing assembly directory.");

        return Path.Combine(applicationPath, RelativePath);
    }
}
