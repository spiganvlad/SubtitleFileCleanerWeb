﻿using Asp.Versioning;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class ApiVersioningRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddApiVersioning(config =>
        {
            config.ReportApiVersions = true;
            config.ApiVersionReader = new HeaderApiVersionReader("api-version");

            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
        })
        .AddApiExplorer(config =>
        {
            config.GroupNameFormat = "'v'VVV";
            config.SubstituteApiVersionInUrl = true;
        });
    }
}
