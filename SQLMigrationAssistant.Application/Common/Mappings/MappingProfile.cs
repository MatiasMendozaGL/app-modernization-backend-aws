using AutoMapper;
using SQLMigrationAssistant.Application.DTOs;
using SQLMigrationAssistant.Domain.Entities;

namespace SQLMigrationAssistant.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Migration, MigrationResponse>();

            // Si tuvieras propiedades con nombres diferentes o que necesitaran una lógica especial,
            // lo definirías aquí. Por ejemplo, si LastMigrationExecution fuera un DateTime en el dominio:
            /*
            CreateMap<Migration, MigrationResponse>()
                .ForMember(
                    dest => dest.LastMigrationExecution,
                    opt => opt.MapFrom(src => src.LastMigrationExecution.ToString("o")) // Formato ISO 8601
                );
            */
        }
    }
}
