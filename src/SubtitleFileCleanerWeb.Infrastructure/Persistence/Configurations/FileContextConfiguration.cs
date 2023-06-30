using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Infrastructure.Persistence.Configurations;

public class FileContextConfiguration : IEntityTypeConfiguration<FileContext>
{
    public void Configure(EntityTypeBuilder<FileContext> builder)
    {
        builder.Ignore(f => f.Content);
    }
}
