using SubtitleFileCleanerWeb.Infrastructure.Exceptions;

namespace SubtitleFileCleanerWeb.Infrastructure.Blob;

public class InMemoryBlobStorageContext : IBlobStorageContext
{
    public Dictionary<string, Stream> StorageContext { get; } = new();

    public async Task<Stream?> GetContentStreamAsync(string path, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            if (StorageContext.ContainsKey(path))
                return StorageContext[path];

            return null;
        }, cancellationToken);
    }

    public async Task CreateContentAsync(string path, Stream contentStream, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            if (StorageContext.ContainsKey(path))
                throw new BlobStorageOperationException($"Content already exists on path: {path}");

            StorageContext.Add(path, contentStream);
        }, cancellationToken);
    }

    public async Task DeleteContentAsync(string path, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            if (!StorageContext.ContainsKey(path))
                throw new BlobStorageOperationException($"No blob content was found on path: {path}");

            StorageContext.Remove(path);
        }, cancellationToken);
    }
}
