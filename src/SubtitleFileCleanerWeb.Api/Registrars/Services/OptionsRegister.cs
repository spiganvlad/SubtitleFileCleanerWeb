using SubtitleFileCleanerWeb.Api.Configurations;
using SubtitleFileCleanerWeb.Api.Options;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services
{
    public class OptionsRegister : IServicesRegister
    {
        public void Register(WebApplicationBuilder builder)
        {
            builder.Services.AddOptions<FormFileOptions>()
                .Bind(builder.Configuration.GetRequiredSection(ConfigurationKeys.FormFile));
        }
    }
}
