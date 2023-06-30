using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Exceptions;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.PostConversion;

public class PostConversionProcessor : IPostConversionProcessor
{
    private readonly IEnumerable<IPostConverter> _converters;

    public PostConversionProcessor(IEnumerable<IPostConverter> converters)
    {
        _converters = converters;
    }

    public async Task<OperationResult<Stream>> ProcessAsync(Stream contentStream, ConversionType conversionType,
        CancellationToken cancellationToken, params PostConversionOption[] options)
    {
        var result = new OperationResult<Stream>();

        try
        {
            foreach (var option in options)
            {
                var optionFulfilled = false;

                foreach (var converter in _converters)
                {
                    if (converter.PostConversionOption == option)
                    {
                        contentStream = await converter.ConvertAsync(contentStream, conversionType, cancellationToken);

                        optionFulfilled = true;
                        break;
                    }
                }

                if (!optionFulfilled)
                {
                    result.AddError(ErrorCode.PostConversionException,
                        string.Format(PostConversionErrorMessages.ConverterForOptionNotFound, option));
                    return result;
                }
            }

            result.Payload = contentStream;
        }
        catch (NotConvertibleContentException ex)
        {
            result.AddError(ErrorCode.UnprocessableContent, ex.Message);
        }
        catch (Exception ex)
        {
            result.AddUnknownError(ex.Message);
        }

        return result;
    }
}
