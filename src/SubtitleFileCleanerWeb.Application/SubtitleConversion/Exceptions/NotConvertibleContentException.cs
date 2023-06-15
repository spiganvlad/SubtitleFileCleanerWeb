namespace SubtitleFileCleanerWeb.Application.SubtitleConversion.Exceptions;

public class NotConvertibleContentException : Exception
{
    internal NotConvertibleContentException() { }
    internal NotConvertibleContentException(string message) : base(message) { }
    internal NotConvertibleContentException(string message, Exception innerException) : base(message, innerException) { }
}
