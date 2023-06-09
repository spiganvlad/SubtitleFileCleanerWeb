using MediatR;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Application.FileContexts.Queries;

public record GetFileContextById(Guid FileContextId) : IRequest<OperationResult<FileContext>>;