using SubtitleBytesClearFormatting.Cleaners;
using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.SubtitleConversion.Converters;

public class SrtConverter : BaseConverter<SrtCleaner>
{
    public override ConversionType ConversionType => ConversionType.Srt;
}
