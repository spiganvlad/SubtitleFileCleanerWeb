using MediatR;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Application.FileContexts.Queries;

public record GetFileContextWithContentById(Guid FileContextId) : IRequest<OperationResult<FileContext>>;
