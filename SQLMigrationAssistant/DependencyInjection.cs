using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using SQLMigrationAssistant.API.Filters;
using SQLMigrationAssistant.API.Mapping;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SQLMigrationAssistant.API
{
    public static class DependencyInjection
    {

        public static IServiceCollection AddPresentationServices(this IServiceCollection services)
        {
            services.AddMapper();
            services.AddValidators();
            services.AddApiDocumentation();
            services.AddCorsConfiguration();

            return services;
        }

        #region private

        private static IServiceCollection AddMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));
            return services;
        }
        private static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation();

            // Register all validators from the current assembly
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            return services;
        }

        private static IServiceCollection AddApiDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(ConfigureSwagger);

            return services;
        }

        private static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            return services;
        }

        private static void ConfigureSwagger(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "SQL Migration Assistant API",
                Description = "API for SQL Migration Assistant operations"
            });

            // Add the enum schema filter
            options.SchemaFilter<EnumSchemaFilter>();
            AddJwtAuthentication(options);
        }

        private static void AddJwtAuthentication(SwaggerGenOptions options)
        {
            const string bearerScheme = "Bearer";

            options.AddSecurityDefinition(bearerScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = bearerScheme,
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. " +
                             "Enter 'Bearer' [space] and then your token in the text input below. " +
                             "Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = bearerScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }

        #endregion private
    }
}
