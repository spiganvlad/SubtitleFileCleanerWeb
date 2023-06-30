using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.Abstractions;

public interface ITagRemover
{
    public ConversionType ConversionType { get; }

    public Task<Stream> RemoveTagsAsync(Stream contentStream, CancellationToken cancellationToken);
}
