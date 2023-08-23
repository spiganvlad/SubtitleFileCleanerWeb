using SubtitleBytesClearFormatting.Cleaners;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.SubtitleConversion.Converters;

public abstract class BaseConverter<TCleaner> : ISubtitleConverter
    where TCleaner : ISubtitleCleanerAsync, new()
{
    public abstract ConversionType ConversionType { get; }

    public virtual async Task<OperationResult<Stream>> ConvertAsync(Stream contentStream, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Stream>();

        try
        {
            var content = new byte[contentStream.Length];
            await contentStream.ReadAsync(content, cancellationToken);

            var convertedContent = await new TCleaner().DeleteFormattingAsync(content);
            if (convertedContent.Count == 0)
            {
                result.AddError(ErrorCode.UnprocessableContent, SubtitleConversionErrorMessages.NoContentProduced);
                return result;
            }

            result.Payload = new MemoryStream(convertedContent.ToArray(), false);
        }
        catch (Exception ex)
        {
            result.AddUnknownError(ex.Message);
        }

        return result;
    }
}
