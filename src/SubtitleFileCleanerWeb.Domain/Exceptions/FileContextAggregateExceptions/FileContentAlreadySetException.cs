namespace SubtitleFileCleanerWeb.Domain.Exceptions.FileContextAggregateExceptions;

public class FileContentAlreadySetException : Exception
{
    internal FileContentAlreadySetException() { }
    internal FileContentAlreadySetException(string message) : base(message) { }
    internal FileContentAlreadySetException(string message, Exception innerException) : base(message, innerException) { }
}
