using SubtitleBytesClearFormatting.TagsGenerate;
using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.PostConversion.TagRemovers;

public class SubTagRemover : BaseTagRemover
{
    public override ConversionType ConversionType => ConversionType.Sub;

    protected override Dictionary<byte, List<TxtTag>> GetTags() =>
        TagsCollectionGeneretor.GetSubSpecificTags();
}
