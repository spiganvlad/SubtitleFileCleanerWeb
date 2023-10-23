using SubtitleFileCleanerWeb.Api.Options;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class SwaggerRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen();
        builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();
    }
}
