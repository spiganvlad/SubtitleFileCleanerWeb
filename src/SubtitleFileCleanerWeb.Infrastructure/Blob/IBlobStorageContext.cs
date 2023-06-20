namespace SubtitleFileCleanerWeb.Infrastructure.Blob
{
    public interface IBlobStorageContext
    {
        /// <summary>
        /// Gets blob stream asynchronously on the specified path
        /// </summary>
        /// <param name="path">The path where the blob stream will be received</param>
        /// <param name="cancellationToken">The token to monitor for cancellation request</param>
        /// <returns>A task that represents the asynchronous get operation. The task result contains the blob content stream or null</returns>
        public Task<Stream?> GetContentStreamAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously creates blob content at the specified path
        /// </summary>
        /// <param name="path">The path where the blob content will be placed</param>
        /// <param name="contentStream">Readable stream from which blob content will be retrieved</param>
        /// <param name="cancellationToken">The token to monitor for cancellation request</param>
        /// <returns>A task that represents the asynchronous create operation</returns>
        /// <exception cref="Exceptions.BlobStorageOperationException"></exception>
        public Task CreateContentAsync(string path, Stream contentStream, CancellationToken cancellationToken);
    }
}
