using AutoMapper;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Responses;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Api.MappingProfiles;

public class FileContextMapping : Profile
{
    public FileContextMapping()
    {
        CreateMap<FileContext, FileContextResponse>();
    }
}
