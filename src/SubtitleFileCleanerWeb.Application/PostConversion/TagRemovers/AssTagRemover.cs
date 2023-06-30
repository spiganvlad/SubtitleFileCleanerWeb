using SubtitleBytesClearFormatting.TagsGenerate;
using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.PostConversion.TagRemovers;

public class AssTagRemover : BaseTagRemover
{
    public override ConversionType ConversionType => ConversionType.Ass;

    protected override Dictionary<byte, List<TxtTag>> GetTags() =>
        TagsCollectionGeneretor.GetAssSpecificTags();
}
