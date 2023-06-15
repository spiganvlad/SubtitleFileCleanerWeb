using SubtitleBytesClearFormatting.Cleaners;
using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.SubtitleConversion.Converters;

public class SbvConverter : BaseConverter<SbvCleaner>
{
    public override ConversionType ConversionType => ConversionType.Sbv;
}
