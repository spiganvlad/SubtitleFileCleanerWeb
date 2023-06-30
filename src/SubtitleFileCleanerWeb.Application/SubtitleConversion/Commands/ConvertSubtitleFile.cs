using MediatR;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.SubtitleConversion.Commands;

public record ConvertSubtitleFile(Stream ContentStream, ConversionType ConversionType) : IRequest<OperationResult<Stream>>;