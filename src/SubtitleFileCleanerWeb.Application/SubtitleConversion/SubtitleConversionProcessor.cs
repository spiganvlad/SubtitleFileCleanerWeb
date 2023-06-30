using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Exceptions;
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
                        result.Payload = await c.ConvertAsync(contentStream, cancellationToken);
                        return result;
                    }
                }

                result.AddError(ErrorCode.SubtitleConversionException,
                    string.Format(SubtitleConversionErrorMessages.SubtitleConverterNotFound, conversionType));
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
}
