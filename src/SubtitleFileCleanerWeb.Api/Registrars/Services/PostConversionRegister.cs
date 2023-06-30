using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.PostConversion;
using SubtitleFileCleanerWeb.Application.PostConversion.PostConverters;
using SubtitleFileCleanerWeb.Application.PostConversion.TagRemovers;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class PostConversionRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<ITagRemover, AssTagRemover>();
        builder.Services.AddTransient<ITagRemover, SbvTagRemover>();
        builder.Services.AddTransient<ITagRemover, SrtTagRemover>();
        builder.Services.AddTransient<ITagRemover, SubTagRemover>();
        builder.Services.AddTransient<ITagRemover, VttTagRemover>();

        builder.Services.AddTransient<IPostConverter, DeleteTagsConverter>();
        builder.Services.AddTransient<IPostConverter, ToOneLineConverter>();

        builder.Services.AddTransient<IPostConversionProcessor, PostConversionProcessor>();
    }
}
