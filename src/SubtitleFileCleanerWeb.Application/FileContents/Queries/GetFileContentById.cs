using MediatR;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Application.FileContents.Queries;

public record GetFileContentById(string Path) : IRequest<OperationResult<FileContent>>;
