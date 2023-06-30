using SubtitleBytesClearFormatting.Cleaners;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Exceptions;

namespace SubtitleFileCleanerWeb.Application.PostConversion.PostConverters;

public class ToOneLineConverter : IPostConverter
{
    public PostConversionOption PostConversionOption => PostConversionOption.ToOneLine;

    public async Task<Stream> ConvertAsync(Stream contentStream, ConversionType _, CancellationToken cancellationToken)
    {
        if (!contentStream.CanRead || contentStream.Length == 0)
            throw new NotConvertibleContentException();//Task<Stream> => Task<OperationResult<Stream>>

        var content = new byte[contentStream.Length];
        await contentStream.ReadAsync(content, cancellationToken);

        var result = TxtCleaner.ToOneLine(content);

        return new MemoryStream(result, false);
    }
}
