namespace SubtitleFileCleanerWeb.Api.Registrars.Pipelines;

public class MvcRegister : IPipelineRegister
{
    public void Register(WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();
    }
}
