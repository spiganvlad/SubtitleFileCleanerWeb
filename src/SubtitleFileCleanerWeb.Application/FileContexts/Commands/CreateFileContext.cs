using MediatR;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Application.FileContexts.Commands;

public record CreateFileContext(string FileName, Stream ContentStream) : IRequest<OperationResult<FileContext>>;