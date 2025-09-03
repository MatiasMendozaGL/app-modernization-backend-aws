using AutoMapper;
using SQLMigrationAssistant.API.Models;
using SQLMigrationAssistant.Application.DTOs;
using SQLMigrationAssistant.Domain.Enums;

namespace SQLMigrationAssistant.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ConvertRequest, MigrateRequest>()
                .ForMember(dest => dest.FileContent, opt => opt.MapFrom(src => src.File != null ? src.File.OpenReadStream() : null))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.File != null ? src.File.FileName : null))
                .ForMember(dest => dest.FileContentType, opt => opt.MapFrom(src => src.File != null ? src.File.ContentType : null))
                .ForMember(dest => dest.LlmProvideType, opt => opt.MapFrom(src => src.LLMProvider))
                .ForMember(dest => dest.TargetLanguage, opt => opt.MapFrom(src => src.TargetLanguage))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom((src, dest, destMember, context) => context.Items["UserId"]));
        }
    }
}
