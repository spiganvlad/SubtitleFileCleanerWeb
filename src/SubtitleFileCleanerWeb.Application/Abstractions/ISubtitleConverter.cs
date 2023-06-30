using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.Abstractions;

public interface ISubtitleConverter
{
    public ConversionType ConversionType { get; }

    public Task<Stream> ConvertAsync(Stream contentStream, CancellationToken cancellationToken);
}
