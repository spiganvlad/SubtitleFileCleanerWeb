namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public interface IServicesRegister : IRegister
{
    public void Register(WebApplicationBuilder builder);
}