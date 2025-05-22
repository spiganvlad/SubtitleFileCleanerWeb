namespace SubtitleFileCleanerWeb.Domain.Exceptions;

public class DomainModelInvalidException : Exception
{
    public List<string> ValidationErrors { get; }

    internal DomainModelInvalidException()
    {
        ValidationErrors = [];
    }

    internal DomainModelInvalidException(string message): base(message)
    {
        ValidationErrors = [];
    }

    internal DomainModelInvalidException(string message, Exception innerException): base(message, innerException)
    {
        ValidationErrors = [];
    }
}
