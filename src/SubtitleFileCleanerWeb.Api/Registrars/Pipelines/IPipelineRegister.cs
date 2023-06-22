namespace SubtitleFileCleanerWeb.Api.Registrars.Pipelines;

public interface IPipelineRegister : IRegister
{
    public void Register(WebApplication app);
}