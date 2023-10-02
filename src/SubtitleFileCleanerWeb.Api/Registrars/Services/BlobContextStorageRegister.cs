using SubtitleFileCleanerWeb.Api.Configurations;
using SubtitleFileCleanerWeb.Infrastructure.Blob;
using SubtitleFileCleanerWeb.Infrastructure.Blob.FileSystem;
using SubtitleFileCleanerWeb.Infrastructure.Blob.InMemory;

namespace SubtitleFileCleanerWeb.Api.Registrars.Services;

public class BlobContextStorageRegister : IServicesRegister
{
    public void Register(WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSingleton<IBlobStorageContext, InMemoryBlobStorageContext>();
        }
        else
        {
            builder.Services.AddOptions<FileSystemStorageOptions>()
                .Bind(builder.Configuration.GetSection(ConfigurationKeys.FileSystemStorage))
                .Validate(options => !string.IsNullOrWhiteSpace(options.RelativePath), 
                "Relative storage path cannot be null or white space.")
                .ValidateOnStart();

            builder.Services.AddSingleton<IBlobStorageContext, FileSystemStorageContext>();
        }
    }
}
