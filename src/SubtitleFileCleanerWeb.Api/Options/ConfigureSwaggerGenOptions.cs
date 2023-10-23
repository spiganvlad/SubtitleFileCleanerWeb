using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SubtitleFileCleanerWeb.Api.Options;

public class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _apiProvider;

    public ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider apiProvider)
    {
        _apiProvider = apiProvider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _apiProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateOpenApiInfo(description));
        }
    }

    private OpenApiInfo CreateOpenApiInfo(ApiVersionDescription description)
    {
        var apiInfo = new OpenApiInfo
        {
            Title = "SubtitleFileCleanerWeb",
            Version = description.ApiVersion.ToString()
        };

        if (description.IsDeprecated)
            apiInfo.Description = "This api version is deprecated";

        return apiInfo;
    }
}
