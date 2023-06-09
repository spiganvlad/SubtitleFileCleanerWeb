using MediatR;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Application.FileContexts.Commands;

public record UpdateFileContextName(Guid FileContextId, string FileName) : IRequest<OperationResult<FileContext>>;