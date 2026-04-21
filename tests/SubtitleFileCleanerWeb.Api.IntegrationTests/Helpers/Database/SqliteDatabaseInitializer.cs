using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Factories;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Database;

public class SqliteDatabaseInitializer : ITestServiceInitializer
{
    private readonly string _connectionString = $"Data Source={Guid.NewGuid()}.sqlite;";
    private readonly List<object> _seedData = [];
    private ApplicationDbContext? _dbContext;
    private bool _isDisposed;

    public void Initialize(IServiceCollection services)
    {
        InitializeDatabase(services);

        if (_seedData.Count > 0)
            InitializeSeedData();
    }

    public void InitializeDatabase(IServiceCollection services)
    {
        services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
        services.AddSqlite<ApplicationDbContext>(_connectionString);

        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

        dbContext!.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        _dbContext = dbContext;
    }

    public void InitializeSeedData()
    {
        _dbContext!.AddRange(_seedData);
        _dbContext.SaveChanges();
    }

    public void AddSeedData(object data) => _seedData.Add(data);

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _dbContext?.Database.EnsureDeleted();
        _isDisposed = true;
    }
}
