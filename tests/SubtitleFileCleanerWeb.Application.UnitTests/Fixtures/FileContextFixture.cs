using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Fixtures;

public static class FileContextFixture
{
    public static List<FileContext> GetListOfThree()
    {
        return
        [
            FileContext.Create("FirstFileName", 1),
            FileContext.Create("SecondFileName", 2),
            FileContext.Create("ThirdFileName", 3)
        ];
    }
}
