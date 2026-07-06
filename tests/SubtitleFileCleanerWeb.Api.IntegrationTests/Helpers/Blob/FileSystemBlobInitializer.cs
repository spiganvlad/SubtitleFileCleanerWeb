using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Factories;
using SubtitleFileCleanerWeb.Infrastructure.Blob;
using SubtitleFileCleanerWeb.Infrastructure.Blob.FileSystem;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Blob;

public class FileSystemBlobInitializer : ITestServiceInitializer
{
    private readonly string _relativeStoragePath;
    private readonly string _fullStoragePath;
    private readonly Dictionary<Guid, byte[]> _seedData = [];
    private bool _isDisposed;

    public FileSystemBlobInitializer()
    {
        _relativeStoragePath = $"testBlobStorage-{Guid.NewGuid()}";

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

        if (assemblyDirectory is null)
            throw new InvalidOperationException("Could not determine the directory of the executing assembly.");

        _fullStoragePath = Path.Combine(assemblyDirectory, _relativeStoragePath);
    }

    public void Initialize(IServiceCollection services)
    {
        InitializeBlob(services);

        if (_seedData.Count > 0)
            InitializeSeedData(services);
    }

    private void InitializeBlob(IServiceCollection services)
    {
        services.RemoveAll<IBlobStorageContext>();

        var fileSystemStorageOptions = new FileSystemStorageOptions() { RelativePath = _relativeStoragePath };
        var blobOptions = Microsoft.Extensions.Options.Options.Create(fileSystemStorageOptions);

        services.AddSingleton<IBlobStorageContext, FileSystemStorageContext>();
        services.AddSingleton(blobOptions);
    }

    private void InitializeSeedData(IServiceCollection services)
    {
        using var provider = services.BuildServiceProvider();
        var blobContext = provider.GetService<IBlobStorageContext>();

        foreach (var data in _seedData)
        {
            var path = Path.Combine("Unauthorized", data.Key.ToString());
            using var stream = new MemoryStream(data.Value);

            blobContext!.CreateContentAsync(path, stream, default);
        }
    }

    public void AddSeedData(Guid guidId, byte[] content) => _seedData.Add(guidId, content);

    public void Dispose()
    {
        if (_isDisposed)
            return;

        if (Directory.Exists(_fullStoragePath))
            Directory.Delete(_fullStoragePath, true);

        _isDisposed = true;
    }
}
