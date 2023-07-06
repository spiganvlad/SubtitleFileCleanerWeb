using SubtitleFileCleanerWeb.Api.Filters;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class MvcRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ApiExceptionFilterAttribute>();
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        
    }
}
