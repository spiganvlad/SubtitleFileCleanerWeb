namespace SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Responses;

public class FileContextResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public long Size { get; set; }
}
