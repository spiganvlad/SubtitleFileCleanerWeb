using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Database;

public static class DatabaseHelper
{
    public static async Task<FileContext?> GetSavedContextAsync(Guid id, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.FileContexts.SingleOrDefaultAsync((x) => x.FileContextId == id, cancellationToken);
    }
}
