using MediatR;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.PostConversion.Commands;

public record PostConvertFile(Stream ContentStream, ConversionType ConversionType,
    PostConversionOption[] ConversionOptions) : IRequest<OperationResult<Stream>>;