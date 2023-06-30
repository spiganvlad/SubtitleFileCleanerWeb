using SubtitleBytesClearFormatting.TagsGenerate;
using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.PostConversion.TagRemovers;

public class SrtTagRemover : BaseTagRemover
{
    public override ConversionType ConversionType => ConversionType.Srt;

    protected override Dictionary<byte, List<TxtTag>> GetTags() =>
        TagsCollectionGeneretor.GetBasicTags();
}
