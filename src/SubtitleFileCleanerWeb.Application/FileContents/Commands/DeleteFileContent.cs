using MediatR;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.FileContents.Commands;

public record DeleteFileContent(string Path) : IRequest<OperationResult<bool>>;