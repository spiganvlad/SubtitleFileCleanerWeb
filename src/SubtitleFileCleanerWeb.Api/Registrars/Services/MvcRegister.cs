namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class MvcRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }
}
