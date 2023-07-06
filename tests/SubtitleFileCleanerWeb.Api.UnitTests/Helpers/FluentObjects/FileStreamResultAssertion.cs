using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

public class FileStreamResultAssertion : ObjectAssertions<FileStreamResult, FileStreamResultAssertion>
{
    public FileStreamResultAssertion(FileStreamResult fileStreamResult) : base(fileStreamResult) { }
}
