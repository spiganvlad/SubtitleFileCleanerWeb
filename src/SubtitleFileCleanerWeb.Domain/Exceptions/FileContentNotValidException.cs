namespace SubtitleFileCleanerWeb.Domain.Exceptions;

public class FileContentNotValidException : DomainModelInvalidException
{
    internal FileContentNotValidException() { }
    internal FileContentNotValidException(string message) : base(message) { }
    internal FileContentNotValidException(string message, Exception innerException) : base(message, innerException) { }
}
