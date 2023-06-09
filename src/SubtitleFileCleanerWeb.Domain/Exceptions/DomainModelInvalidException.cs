namespace SubtitleFileCleanerWeb.Domain.Exceptions;

public class DomainModelInvalidException : Exception
{
    public List<string> ValidationErrors { get; }

    internal DomainModelInvalidException()
    {
        ValidationErrors = new List<string>();
    }

    internal DomainModelInvalidException(string message): base(message)
    {
        ValidationErrors = new List<string>();
    }

    internal DomainModelInvalidException(string message, Exception innerException): base(message, innerException)
    {
        ValidationErrors = new List<string>();
    }
}
