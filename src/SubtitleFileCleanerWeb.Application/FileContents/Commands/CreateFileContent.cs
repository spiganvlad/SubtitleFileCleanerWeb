using MediatR;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Application.FileContents.Commands;

public record CreateFileContent(string Path, Stream ContentStream) : IRequest<OperationResult<FileContent>>;
