using MediatR;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Application.FileContents.Queries;

public record GetFileContentById(Guid FileContextId) : IRequest<OperationResult<FileContent>>;
