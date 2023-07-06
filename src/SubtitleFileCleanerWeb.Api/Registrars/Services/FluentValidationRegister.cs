using FluentValidation;
using FluentValidation.AspNetCore;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class FluentValidationRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddFluentValidationAutoValidation();

        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
    }
}
