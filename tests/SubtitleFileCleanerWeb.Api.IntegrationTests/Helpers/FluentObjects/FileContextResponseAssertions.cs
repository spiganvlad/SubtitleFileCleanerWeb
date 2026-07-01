using AwesomeAssertions.Execution;
using AwesomeAssertions.Primitives;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Responses;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.FluentObjects;

public class FileContextResponseAssertions : ObjectAssertions<FileContextResponse, FileContextResponseAssertions>
{
    public FileContextResponseAssertions(FileContextResponse response, AssertionChain assertionChain)
        : base(response, assertionChain) { }

    public AndConstraint<FileContextResponseAssertions> HaveId(Guid id)
    {
        Subject.Id.Should().Be(id);

        return new AndConstraint<FileContextResponseAssertions>(this);
    }

    public AndConstraint<FileContextResponseAssertions> HaveName(string name)
    {
        Subject.Name.Should().Be(name);

        return new AndConstraint<FileContextResponseAssertions>(this);
    }

    public AndConstraint<FileContextResponseAssertions> HaveSize(long size)
    {
        Subject.Size.Should().Be(size);

        return new AndConstraint<FileContextResponseAssertions>(this);
    }
}
