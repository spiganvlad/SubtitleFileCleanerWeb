using Microsoft.EntityFrameworkCore;
using SubtitleFileCleanerWeb.Api.Configurations;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class DatabaseContextRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("DevelopmentDB"));
        }
        else
        {
            var connectionString = builder.Configuration.GetConnectionString(ConfigurationKeys.SQLiteConnectionString);
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
        }
    }
}
