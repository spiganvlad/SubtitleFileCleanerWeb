using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.Abstractions;

public interface IPostConverter
{
    public PostConversionOption PostConversionOption { get; }

    public Task<Stream> ConvertAsync(Stream contentStream, ConversionType conversionType, CancellationToken cancellationToken);
}
