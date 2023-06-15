using SubtitleBytesClearFormatting.Cleaners;
using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.SubtitleConversion.Converters;

public class VttConverter : BaseConverter<VttCleaner>
{
    public override ConversionType ConversionType => ConversionType.Vtt;
}
