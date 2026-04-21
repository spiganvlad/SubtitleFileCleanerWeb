using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Factories;

public class SubtitleFileCleanerWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly List<ITestServiceInitializer> serviceInitializers = [];

    public SubtitleFileCleanerWebApplicationFactory(params ITestServiceInitializer[] initializers)
    {
        foreach (var initializer in initializers)
        {
            ArgumentNullException.ThrowIfNull(initializer);
            serviceInitializers.Add(initializer);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (serviceInitializers.Count == 0)
            return;

        builder.ConfigureTestServices(services =>
        {
            foreach (var initializer in serviceInitializers)
                initializer.Initialize(services);
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && serviceInitializers.Count > 0)
        {
            foreach (var initializer in serviceInitializers)
                initializer.Dispose();
        }

        base.Dispose(disposing);
    }
}
