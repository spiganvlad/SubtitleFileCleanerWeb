using SubtitleBytesClearFormatting.TagsGenerate;
using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.PostConversion.TagRemovers;

public class SbvTagRemover : BaseTagRemover
{
    public override ConversionType ConversionType => ConversionType.Sbv;

    protected override Dictionary<byte, List<TxtTag>> GetTags() =>
        TagsCollectionGeneretor.GetBasicTags();
}
