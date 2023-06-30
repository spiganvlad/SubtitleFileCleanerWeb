using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.PostConversion.PostConverters;

public class DeleteTagsConverter : IPostConverter
{
    private readonly IEnumerable<ITagRemover> _tagRemovers;

    public PostConversionOption PostConversionOption => PostConversionOption.DeleteTags;

    public DeleteTagsConverter(IEnumerable<ITagRemover> tagRemovers)
    {
        _tagRemovers = tagRemovers;
    }

    public async Task<Stream> ConvertAsync(Stream contentStream, ConversionType conversionType, CancellationToken cancellationToken)
    {
        foreach (var remover in _tagRemovers)
        {
            if (remover.ConversionType == conversionType)
            {
                return await remover.RemoveTagsAsync(contentStream, cancellationToken);
            }
        }

        throw new Exception();//Task<Stream> => Task<OperationResult<Stream>>
    }
}
