using SubtitleFileCleanerWeb.Application;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class MediatorRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(typeof(ApplicationMediatREntryPoint).Assembly);
        });
    }
}