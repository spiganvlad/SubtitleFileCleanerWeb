using SubtitleFileCleanerWeb.Infrastructure.Blob;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class BlobContextStorageRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSingleton<IBlobStorageContext, InMemoryBlobStorageContext>();
        }
    }
}