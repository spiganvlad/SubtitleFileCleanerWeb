using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext
{
    public virtual DbSet<FileContext> FileContexts => Set<FileContext>();

    public ApplicationDbContext() { }

    public ApplicationDbContext(DbContextOptions options): base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }
}
