using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

public class FileStreamResultAssertion : ObjectAssertions<FileStreamResult, FileStreamResultAssertion>
{
    private readonly AndConstraint<FileStreamResultAssertion> _andConstraintThis;

    public FileStreamResultAssertion(FileStreamResult fileStreamResult) : base(fileStreamResult)
    {
        _andConstraintThis = new AndConstraint<FileStreamResultAssertion>(this);
    }

    public AndConstraint<FileStreamResultAssertion> HaveContentType(string contentType)
    {
        Subject.ContentType.Should().NotBeNull().And.Be(contentType);

        return _andConstraintThis;
    }

    public AndConstraint<FileStreamResultAssertion> HaveFileDownloadName(string fileDownloadName)
    {
        Subject.FileDownloadName.Should().NotBeNull().And.Be(fileDownloadName);

        return _andConstraintThis;
    }

    public AndConstraint<FileStreamResultAssertion> HaveDisabledRangeProcessing()
    {
        Subject.EnableRangeProcessing.Should().BeFalse();

        return _andConstraintThis;
    }

    public AndWhichConstraint<FileStreamResultAssertion, Stream> HaveNotNullStream()
    {
        Subject.FileStream.Should().NotBeNull();

        return new AndWhichConstraint<FileStreamResultAssertion, Stream>(this, Subject.FileStream);
    }
}
