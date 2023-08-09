namespace SubtitleFileCleanerWeb.Api.Registrars.Pipelines;

public class MvcRegister : IPipelineRegister
{
    public void Register(WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseCors();

        app.UseAuthorization();

        app.MapControllers();
    }
}
