using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests;

public record FileContextCreateConverted(IFormFile File, params PostConversionOption[]? PostConversionOptions);
