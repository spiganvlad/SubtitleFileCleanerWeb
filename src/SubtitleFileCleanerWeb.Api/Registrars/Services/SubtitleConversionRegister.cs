using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.SubtitleConversion;
using SubtitleFileCleanerWeb.Application.SubtitleConversion.Converters;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class SubtitleConversionRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<ISubtitleConverter, AssConverter>();
        builder.Services.AddTransient<ISubtitleConverter, SbvConverter>();
        builder.Services.AddTransient<ISubtitleConverter, SrtConverter>();
        builder.Services.AddTransient<ISubtitleConverter, SubConverter>();
        builder.Services.AddTransient<ISubtitleConverter, VttConverter>();

        builder.Services.AddTransient<ISubtitleConversionProcessor, SubtitleConversionProcessor>();
    }
}
