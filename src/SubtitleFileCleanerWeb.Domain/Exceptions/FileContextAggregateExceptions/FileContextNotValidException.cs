namespace SubtitleFileCleanerWeb.Domain.Exceptions.FileContextAggregateExceptions;

public class FileContextNotValidException : DomainModelInvalidException
{
    internal FileContextNotValidException() { }
    internal FileContextNotValidException(string message) : base(message) { }
    internal FileContextNotValidException(string message, Exception innerException) : base(message, innerException) { }
}
