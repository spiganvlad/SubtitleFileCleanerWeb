using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.PostConversion;

public class PostConversionProcessor : IPostConversionProcessor
{
    private readonly IEnumerable<IPostConverter> _converters;

    public PostConversionProcessor(IEnumerable<IPostConverter> converters)
    {
        _converters = converters;
    }

    public async Task<OperationResult<Stream>> ProcessAsync(Stream contentStream,
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
                        var conversionResult = await converter.ConvertAsync(contentStream, cancellationToken);
                        if (conversionResult.IsError)
                        {
                            result.CopyErrors(conversionResult.Errors);
                            return result;
                        }

                        contentStream = conversionResult.Payload!;
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
        catch (Exception ex)
        {
            result.AddUnknownError(ex.Message);
        }

        return result;
    }
}
