﻿using SubtitleFileCleanerWeb.Infrastructure.Exceptions;

namespace SubtitleFileCleanerWeb.Infrastructure.Blob.InMemory;

public class InMemoryBlobStorageContext : IBlobStorageContext
{
    public Dictionary<string, byte[]> StorageContext { get; } = new();

    public async Task<Stream?> GetContentStreamAsync(string path, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            if (StorageContext.ContainsKey(path))
                return new MemoryStream(StorageContext[path], false);

            return null;
        }, cancellationToken);
    }

    public async Task CreateContentAsync(string path, Stream contentStream, CancellationToken cancellationToken)
    {
        if (StorageContext.ContainsKey(path))
            throw new BlobStorageOperationException($"Content already exists on path: {path}.");

        var content = new byte[contentStream.Length];
        await contentStream.ReadAsync(content, cancellationToken);

        StorageContext.Add(path, content);
    }

    public async Task DeleteContentAsync(string path, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            if (!StorageContext.ContainsKey(path))
                throw new BlobStorageOperationException($"No blob content was found on path: {path}.");

            StorageContext.Remove(path);
        }, cancellationToken);
    }
}
