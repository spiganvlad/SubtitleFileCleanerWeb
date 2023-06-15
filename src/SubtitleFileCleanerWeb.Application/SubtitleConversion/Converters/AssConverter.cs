using SubtitleBytesClearFormatting.Cleaners;
using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.SubtitleConversion.Converters;

public class AssConverter : BaseConverter<AssCleaner>
{
    public override ConversionType ConversionType => ConversionType.Ass;
}
