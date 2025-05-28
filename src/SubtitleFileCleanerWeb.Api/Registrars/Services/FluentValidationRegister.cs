using FluentValidation;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class FluentValidationRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
    }
}
