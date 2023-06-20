namespace SubtitleFileCleanerWeb.Infrastructure.Exceptions;

public class BlobStorageOperationException : Exception
{
    internal BlobStorageOperationException() : base() { }
    internal BlobStorageOperationException(string message) : base(message) { }
    internal BlobStorageOperationException(string message, Exception innerException) : base(message, innerException) { }
}
