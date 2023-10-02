using Microsoft.Extensions.Options;

namespace SubtitleFileCleanerWeb.Infrastructure.Blob.FileSystem;

public class FileSystemStorageContext : IBlobStorageContext
{
    private readonly string _rootPath;

    public FileSystemStorageContext(IOptions<FileSystemStorageOptions> options)
    {
        _rootPath = options.Value.GetFullPath();
    }

    public async Task CreateContentAsync(string path, Stream contentStream, CancellationToken cancellationToken)
    {
        var fileInfo = new FileInfo(Path.Combine(_rootPath, path));
        fileInfo.Directory?.Create();

        using var fileStream = fileInfo.OpenWrite();

        contentStream.Position = 0;
        await contentStream.CopyToAsync(fileStream, cancellationToken);
    }

    public async Task DeleteContentAsync(string path, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            File.Delete(Path.Combine(_rootPath, path));
        }, cancellationToken);
    }

    public async Task<Stream?> GetContentStreamAsync(string path, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            var fileInfo = new FileInfo(Path.Combine(_rootPath, path));

            if (fileInfo.Exists)
                return fileInfo.OpenRead();

            return null;
        }, cancellationToken);
    }
}
