using SubtitleBytesClearFormatting.TagsGenerate;
using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.PostConversion.TagRemovers
{
    public class VttTagRemover : BaseTagRemover
    {
        public override ConversionType ConversionType => ConversionType.Vtt;

        protected override Dictionary<byte, List<TxtTag>> GetTags() => 
            TagsCollectionGeneretor.GetBasicTags();
    }
}
