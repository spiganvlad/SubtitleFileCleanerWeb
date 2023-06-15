using SubtitleBytesClearFormatting.Cleaners;
using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.SubtitleConversion.Converters;

public class SubConverter : BaseConverter<SubCleaner>
{
    public override ConversionType ConversionType => ConversionType.Sub;
}
