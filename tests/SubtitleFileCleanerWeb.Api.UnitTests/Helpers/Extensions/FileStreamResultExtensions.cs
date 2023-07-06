using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;

public static class FileStreamResultExtensions
{
    public static FileStreamResultAssertion Should(this FileStreamResult result)
    {
        return new FileStreamResultAssertion(result);
    }

    public static AndConstraint<FileStreamResultAssertion> HaveContentType(this FileStreamResultAssertion assertion, string contentType)
    {
        assertion.Subject.ContentType.Should().NotBeNull().And.Be(contentType);
        return new AndConstraint<FileStreamResultAssertion>(assertion);
    }

    public static AndConstraint<FileStreamResultAssertion> HaveFileDownloadName(this FileStreamResultAssertion assertion, string fileDownloadName)
    {
        assertion.Subject.FileDownloadName.Should().NotBeNull().And.Be(fileDownloadName);
        return new AndConstraint<FileStreamResultAssertion>(assertion);
    }

    public static AndConstraint<FileStreamResultAssertion> HaveDisabledRangeProcessing(this FileStreamResultAssertion assertion)
    {
        assertion.Subject.EnableRangeProcessing.Should().BeFalse();
        return new AndConstraint<FileStreamResultAssertion>(assertion);
    }

    public static AndWhichConstraint<FileStreamResultAssertion, Stream> HaveNotNullStream(this FileStreamResultAssertion assertion)
    {
        assertion.Subject.FileStream.Should().NotBeNull();
        return new AndWhichConstraint<FileStreamResultAssertion, Stream>(assertion, assertion.Subject.FileStream);
    }
}
