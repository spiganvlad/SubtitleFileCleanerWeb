using MediatR;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Application.FileContents.Commands;

public record CreateFileContent(Guid FileContextId, Stream ContentStream) : IRequest<OperationResult<FileContent>>;
