namespace SubtitleFileCleanerWeb.Api.Registrars.Pipelines;

public class SwaggerRegister : IPipelineRegister
{
    public void Register(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}
