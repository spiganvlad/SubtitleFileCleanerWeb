using MediatR;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Application.FileContexts.Commands;

public record DeleteFileContext(Guid FileContextId) : IRequest<OperationResult<FileContext>>;