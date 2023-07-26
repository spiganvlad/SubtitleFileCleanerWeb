using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.PostConversion;
using SubtitleFileCleanerWeb.Application.PostConversion.PostConverters;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class PostConversionRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IPostConverter, DeleteAssTagsConverter>();
        builder.Services.AddTransient<IPostConverter, DeleteBasicTagsConverter>();
        builder.Services.AddTransient<IPostConverter, DeleteSubTagsConverter>();
        builder.Services.AddTransient<IPostConverter, ToOneLineConverter>();

        builder.Services.AddTransient<IPostConversionProcessor, PostConversionProcessor>();
    }
}
