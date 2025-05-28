using Asp.Versioning.ApiExplorer;

namespace SubtitleFileCleanerWeb.Api.Registrars.Pipelines;

public class SwaggerRegister : IPipelineRegister
{
    public void Register(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var apiProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in apiProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                        $"SubtitleFileCleanerWeb.Api {description.GroupName}");
                }
            });
        }
    }
}
