namespace SubtitleFileCleanerWeb.Infrastructure.Blob
{
    public interface IBlobStorageContext
    {
        public Task<Stream?> GetContentStreamAsync(string path);
        public Task CreateContentAsync(string path, Stream contentStream);
    }
}
