using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.Models
{
    public class Error
    {
        public ErrorCode Code { get; set; }
        public string? Message { get; set; }
    }
}
