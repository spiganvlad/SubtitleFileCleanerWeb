using Microsoft.Extensions.DependencyInjection;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Factories;

public interface ITestServiceInitializer : IDisposable
{
    void Initialize(IServiceCollection services);
}
