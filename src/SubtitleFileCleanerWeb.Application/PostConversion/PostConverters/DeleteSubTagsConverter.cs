using SubtitleBytesClearFormatting.Cleaners;
using SubtitleBytesClearFormatting.TagsGenerate;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.PostConversion.PostConverters;

public class DeleteSubTagsConverter : IPostConverter
{
    public PostConversionOption PostConversionOption => PostConversionOption.DeleteSubTags;

    public async Task<OperationResult<Stream>> ConvertAsync(Stream contentStream, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Stream>();

        try
        {
            var content = new byte[contentStream.Length];
            await contentStream.ReadAsync(content, cancellationToken);

            var tags = TagsCollectionGeneretor.GetSubSpecificTags();
            var convertedContent = await TxtCleaner.DeleteTagsAsync(content, tags);

            if (convertedContent.Length == 0)
            {
                result.AddError(ErrorCode.UnprocessableContent,
                    PostConversionErrorMessages.NoContentProduced);
                return result;
            }

            result.Payload = new MemoryStream(convertedContent, false);
        }
        catch (Exception ex)
        {
            result.AddUnknownError(ex.Message);
        }

        return result;
    }
}
