using SubtitleBytesClearFormatting.Cleaners;
using SubtitleBytesClearFormatting.TagsGenerate;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Exceptions;

namespace SubtitleFileCleanerWeb.Application.PostConversion.TagRemovers;

public abstract class BaseTagRemover : ITagRemover
{
    public abstract ConversionType ConversionType { get; }
    
    protected abstract Dictionary<byte, List<TxtTag>> GetTags();

    public virtual async Task<Stream> RemoveTagsAsync(Stream contentStream, CancellationToken cancellationToken)
    {
        if (!contentStream.CanRead || contentStream.Length == 0)
            throw new NotConvertibleContentException();//Task<Stream> => Task<OperationResult<Stream>>

        var content = new byte[contentStream.Length];
        await contentStream.ReadAsync(content, cancellationToken);

        var tags = GetTags();
        var resultContent = TxtCleaner.DeleteTags(content, tags);

        return new MemoryStream(resultContent, false);
    }
}
