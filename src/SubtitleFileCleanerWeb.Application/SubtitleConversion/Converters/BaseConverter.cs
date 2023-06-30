using SubtitleBytesClearFormatting.Cleaners;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Exceptions;

namespace SubtitleFileCleanerWeb.Application.SubtitleConversion.Converters;

public abstract class BaseConverter<TCleaner> : ISubtitleConverter
    where TCleaner : ISubtitleCleanerAsync, new()
{
    public abstract ConversionType ConversionType { get; }

    public virtual async Task<Stream> ConvertAsync(Stream contentStream, CancellationToken cancellationToken)
    {
        var content = new byte[contentStream.Length];
        await contentStream.ReadAsync(content, cancellationToken);

        var result = await new TCleaner().DeleteFormattingAsync(content);
        if (result.Count == 0)
            throw new NotConvertibleContentException(SubtitleConversionErrorMessages.NoContentProduced);

        return new MemoryStream(result.ToArray(), false);
    }
}
