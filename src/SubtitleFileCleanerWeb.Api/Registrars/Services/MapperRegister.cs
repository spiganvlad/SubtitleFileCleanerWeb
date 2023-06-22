namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class MapperRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(typeof(Program).Assembly);
    }
}
