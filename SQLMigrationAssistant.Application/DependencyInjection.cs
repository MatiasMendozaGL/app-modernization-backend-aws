using Microsoft.Extensions.DependencyInjection;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Application.Common.Mappings;
using SQLMigrationAssistant.Application.Services;

namespace SQLMigrationAssistant.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IMigrationService, MigrationService>();
            services.AddScoped<IZipService, ZipService>();
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            });



            return services;
        }
    }
}