using SubtitleFileCleanerWeb.Api.Filters;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class MvcRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(config =>
        {
            config.Filters.Add<ApiExceptionFilterAttribute>();
        });
    }
}
