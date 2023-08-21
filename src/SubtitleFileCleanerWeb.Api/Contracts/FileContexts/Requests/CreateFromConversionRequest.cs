using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests;

public record CreateFromConversionRequest(IFormFile File, params PostConversionOption[]? PostConversionOptions);
