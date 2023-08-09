using Microsoft.AspNetCore.Cors.Infrastructure;
using SubtitleFileCleanerWeb.Api.Configurations;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class CorsRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        var policy = builder.Configuration
            .GetSection(ConfigurationKeys.DefaultCorsPolicy).Get<CorsPolicy>();

        builder.Services.AddCors(options =>
        {
            if (policy is not null)
                options.AddDefaultPolicy(policy);
        });
    }
}
