using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.SubtitleConversion
{
    public class SubtitleConversionProcessor : ISubtitleConversionProcessor
    {
        private readonly IEnumerable<ISubtitleConverter> _converters;

        public SubtitleConversionProcessor(IEnumerable<ISubtitleConverter> converters)
        {
            _converters = converters;
        }

        public async Task<OperationResult<Stream>> ProcessAsync(Stream contentStream, ConversionType conversionType,
            CancellationToken cancellationToken)
        {
            var result = new OperationResult<Stream>();

            try
            {
                foreach (var c in _converters)
                {
                    if (c.ConversionType == conversionType)
                    {
                        var conversionResult = await c.ConvertAsync(contentStream, cancellationToken);
                        if (conversionResult.IsError)
                        {
                            result.CopyErrors(conversionResult.Errors);
                            return result;
                        }

                        result.Payload = conversionResult.Payload;
                        return result;
                    }
                }

                result.AddError(ErrorCode.SubtitleConversionException,
                    string.Format(SubtitleConversionErrorMessages.SubtitleConverterNotFound, conversionType));
            }
            catch (Exception ex)
            {
                result.AddUnknownError(ex.Message);
            }

            return result;
        }
    }
}
