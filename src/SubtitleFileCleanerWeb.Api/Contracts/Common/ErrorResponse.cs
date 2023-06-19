namespace SubtitleFileCleanerWeb.Api.Contracts.Common;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string StatusPhrase { get; set; } = null!;
    public List<string> Errors { get; set; } = new List<string>();
    public DateTime Timestamp { get; set; }
}
