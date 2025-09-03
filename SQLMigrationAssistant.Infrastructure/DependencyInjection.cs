using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Domain.Entities;
using SQLMigrationAssistant.Domain.Interfaces;
using SQLMigrationAssistant.Infrastructure.LLM;
using SQLMigrationAssistant.Infrastructure.Repositories;
using SQLMigrationAssistant.Infrastructure.Services;
using SQLMigrationAssistant.Infrastructure.Settings;
using System.Net;
using System.Text;

namespace SQLMigrationAssistant.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthenticationServices();
            services.AddScoped<ICodeBlockProcessor, CodeBlockProcessor>();
            services.AddRetryPolicy(configuration);
            services.AddLLMServices();
            services.AddFileServices();
            services.AddCloudServices(configuration);
            services.AddJwtAuthentication(configuration);

            return services;
        }

        private static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IJwtTokenService, JwtTokenService>();

            return services;
        }

        private static IServiceCollection AddLLMServices(this IServiceCollection services)
        {
            // Register concrete implementations
            services.AddSingleton<OpenAIService>();
            services.AddSingleton<VertexGeminiService>();

            // Register as interfaces for factory pattern
            services.AddSingleton<ILLMService, OpenAIService>(sp => sp.GetRequiredService<OpenAIService>());
            services.AddSingleton<ILLMService, VertexGeminiService>(sp => sp.GetRequiredService<VertexGeminiService>());

            // Register factory and prompt provider
            services.AddSingleton<ILLMServiceFactory, LLMServiceFactory>();
            services.AddSingleton<IPromptProvider, EmbeddedPromptProvider>();

            services.AddScoped<ICodeBlockExtractor, LLMCodeBlockExtractor>();

            return services;
        }

        private static IServiceCollection AddFileServices(this IServiceCollection services)
        {
            //services.AddSingleton<IFileStorageService, CloudStorageService>();
            services.AddScoped<IFileContentReader, StreamFileContentReader>();
            services.AddScoped<IMigrationRepository<Migration>, CloudStorageMigrationRepository>();
            return services;
        }

        private static IServiceCollection AddCloudServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<VertexAiSettings>(configuration.GetSection(VertexAiSettings.SectionName));
            services.Configure<RetrySettings>(configuration.GetSection(RetrySettings.SectionName));
            services.AddGoogleCloudStorage(configuration);

            return services;
        }

        private static IServiceCollection AddRetryPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            // Register RetryPolicy with configuration-based settings
            services.AddSingleton<IRetryPolicy>(serviceProvider =>
            {
                var retrySettings = serviceProvider.GetRequiredService<IOptions<RetrySettings>>().Value;
                var logger = serviceProvider.GetRequiredService<ILogger<RetryPolicy>>();

                // Default retry status codes for cloud storage operations
                var retryStatusCodes = new[]
                {
                    HttpStatusCode.InternalServerError,      // 500
                    HttpStatusCode.BadGateway,               // 502
                    HttpStatusCode.ServiceUnavailable,       // 503
                    HttpStatusCode.GatewayTimeout,           // 504
                    HttpStatusCode.TooManyRequests           // 429
                };

                return new RetryPolicy(
                    maxAttempts: retrySettings.MaxAttempts,
                    initialBackoffSeconds: retrySettings.InitialBackoffSeconds,
                    maxBackoffSeconds: retrySettings.MaxBackoffSeconds,
                    backoffMultiplier: retrySettings.BackoffMultiplier,
                    retryStatusCodes: retryStatusCodes,
                    retryableStatusCodes: retrySettings.RetryableStatusCodes,
                    logger: logger
                );
            });

            return services;
        }

        private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                                ?? throw new InvalidOperationException("JWT settings not configured");

            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            // Configure JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Set to true in production
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();

            return services;
        }

        private static IServiceCollection AddGoogleCloudStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CloudStorageSettings>(options =>
            {
                configuration.GetSection(CloudStorageSettings.SectionName).Bind(options);
                ValidateAndConfigureCloudStorage(options);
            });

            //services.AddSingleton<StorageClient>(_ => StorageClient.Create());
            return services;
        }

        private static void ValidateAndConfigureCloudStorage(CloudStorageSettings options)
        {
            if (string.IsNullOrEmpty(options.BucketName))
            {
                throw new InvalidOperationException("CloudStorageSettings.BucketName is required");
            }

            if (options.MaxFileSizeMB <= 0)
            {
                options.MaxFileSizeMB = 50; // Default value
            }

            if (!string.IsNullOrEmpty(options.CredentialsPath))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", options.CredentialsPath);
            }
        }
    }
}