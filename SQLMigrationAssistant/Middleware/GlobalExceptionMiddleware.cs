using Google;
using Microsoft.AspNetCore.Mvc;
using SQLMigrationAssistant.Application.Common.Exceptions;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using static Google.Rpc.Context.AttributeContext.Types;

namespace SQLMigrationAssistant.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var problemDetails = exception switch
            {
                ArgumentNullException ex => CreateProblemDetails(
                    HttpStatusCode.BadRequest,
                    "Bad Request",
                    $"Required parameter is missing: {ex.ParamName}",
                    context.Request.Path),

                // FluentValidation exceptions (fallback if not caught earlier)
                FluentValidation.ValidationException ex => CreateProblemDetails(
                    HttpStatusCode.BadRequest,
                    "Bad Request",
                    ConvertFluentValidationErrors(ex.Errors),
                    context.Request.Path),

                // Authorization exceptions
                UnauthorizedAccessException ex => CreateProblemDetails(
                    HttpStatusCode.Unauthorized,
                    "Unauthorized",
                    ex.Message,
                    context.Request.Path),

                // Validation exceptions
                ArgumentException ex => CreateProblemDetails(
                    HttpStatusCode.BadRequest,
                    "Bad Request",
                    ex.Message,
                    context.Request.Path),
                // Not found exceptions
                KeyNotFoundException ex => CreateProblemDetails(
                    HttpStatusCode.NotFound,
                    "Not Found",
                    ex.Message,
                    context.Request.Path),

                FileNotFoundException ex => CreateProblemDetails(
                    HttpStatusCode.NotFound,
                    "File Not Found",
                    ex.Message,
                    context.Request.Path),

                // Business logic exceptions
                InvalidOperationException ex => CreateProblemDetails(
                    HttpStatusCode.BadRequest,
                    "Invalid Operation",
                    ex.Message,
                    context.Request.Path),

                // File/IO exceptions
                IOException ex => CreateProblemDetails(
                    HttpStatusCode.InternalServerError,
                    "File Processing Error",
                    "An error occurred while processing the file",
                    context.Request.Path),

                // Timeout exceptions
                TimeoutException ex => CreateProblemDetails(
                    HttpStatusCode.RequestTimeout,
                    "Request Timeout",
                    "The request timed out. Please try again.",
                    context.Request.Path),

                TaskCanceledException ex when ex.InnerException is TimeoutException => CreateProblemDetails(
                    HttpStatusCode.RequestTimeout,
                    "Request Timeout",
                    "The request timed out. Please try again.",
                    context.Request.Path),

                // Database/External service exceptions
                HttpRequestException ex => CreateProblemDetails(
                    HttpStatusCode.BadGateway,
                    "External Service Error",
                    "An error occurred while communicating with external services",
                    context.Request.Path),

                //GloogleCloud Exceptions
                GoogleApiException ex => CreateProblemDetails(
                    MapGoogleApiExceptionToStatusCode(ex),
                    "Cloud Service Error",
                    $"Cloud operation failed: {ex.Message}",
                    context.Request.Path),

                //MigrationException
                MigrationException ex => CreateProblemDetails(
                    HttpStatusCode.InternalServerError,
                    "Migration process failed",
                    ex.Message,
                    context.Request.Path),

                CloudStorageException ex => CreateProblemDetails(
                    HttpStatusCode.InternalServerError,
                    "Cloud Storage Service Error",
                    ex.Message,
                    context.Request.Path),

                // Generic fallback
                _ => CreateProblemDetails(
                    HttpStatusCode.InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred",
                    context.Request.Path)
            };

            response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

            // Log the exception with appropriate level
            LogException(exception, context, response.StatusCode);

            var jsonResponse = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(jsonResponse);
        }

        private async Task HandleFluentValidationException(HttpContext context, MemoryStream memoryStream, Stream originalBodyStream)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            // Try to parse as ValidationProblemDetails
            try
            {
                var validationProblem = JsonSerializer.Deserialize<ValidationProblemDetails>(responseBody);
                if (validationProblem?.Errors?.Any() == true)
                {
                    // Transform validation errors into detail string
                    var validationErrors = validationProblem.Errors.SelectMany(kvp =>
                        kvp.Value.Select(error => new
                        {
                            Field = kvp.Key,
                            Message = error,
                            Code = GenerateErrorCode(kvp.Key, error)
                        })).ToList();

                    // Create detail string from validation errors
                    var detailBuilder = new StringBuilder("Validation failed. Errors: ");
                    detailBuilder.AppendJoin("; ", validationErrors.Select(e => $"{e.Field}: {e.Message}"));

                    // Create and return ProblemDetails
                    var problemDetails = new ProblemDetails
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = "Validation Failed",
                        Detail = detailBuilder.ToString(),
                        Instance = context.Request.Path,
                        Type = $"https://httpstatuses.com/{(int)HttpStatusCode.BadRequest}",
                    };

                    var customJson = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    context.Response.ContentType = "application/json";
                    await originalBodyStream.WriteAsync(Encoding.UTF8.GetBytes(customJson));
                    return;
                }
            }
            catch
            {
                // If parsing fails, return original response
            }

            // Return original response if not a validation problem
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
        }
        private static string GenerateErrorCode(string fieldName, string errorMessage)
        {
            return $"{fieldName.ToUpper()}_{errorMessage.GetHashCode():X}";
        }
        private static HttpStatusCode MapGoogleApiExceptionToStatusCode(GoogleApiException ex)
        {
            return ex.HttpStatusCode switch
            {
                HttpStatusCode.BadRequest => HttpStatusCode.BadRequest,
                HttpStatusCode.Unauthorized => HttpStatusCode.Unauthorized,
                HttpStatusCode.Forbidden => HttpStatusCode.Forbidden,
                HttpStatusCode.NotFound => HttpStatusCode.NotFound,
                HttpStatusCode.Conflict => HttpStatusCode.Conflict,
                HttpStatusCode.TooManyRequests => HttpStatusCode.TooManyRequests,
                HttpStatusCode.InternalServerError => HttpStatusCode.BadGateway, // External service error
                HttpStatusCode.BadGateway => HttpStatusCode.BadGateway,
                HttpStatusCode.ServiceUnavailable => HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.GatewayTimeout => HttpStatusCode.GatewayTimeout,
                _ => HttpStatusCode.BadGateway // Default for external service issues
            };
        }

        private static ProblemDetails CreateProblemDetails(
            HttpStatusCode statusCode,
            string title,
            string detail,
            string instance)
        {
            return new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = detail,
                Instance = instance,
                Type = $"https://httpstatuses.com/{(int)statusCode}"
            };
        }

        private void LogException(Exception exception, HttpContext context, int statusCode)
        {
            var logLevel = statusCode switch
            {
                >= 500 => LogLevel.Error,
                >= 400 and < 500 => LogLevel.Warning,
                _ => LogLevel.Information
            };

            var migrationId = ExtractMigrationIdFromPath(context.Request.Path);
            var fileName = ExtractFileNameFromPath(context.Request.Path);

            // Enhanced logging with migration context
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["RequestMethod"] = context.Request.Method,
                ["RequestPath"] = context.Request.Path.Value ?? "",
                ["StatusCode"] = statusCode,
                ["ExceptionType"] = exception.GetType().Name,
                ["MigrationId"] = migrationId ?? "N/A",
                ["FileName"] = fileName ?? "N/A"
            });

            _logger.Log(logLevel, exception,
                "HTTP {Method} {Path} responded {StatusCode}. MigrationId: {MigrationId}, FileName: {FileName}",
                context.Request.Method,
                context.Request.Path,
                statusCode,
                migrationId ?? "N/A",
                fileName ?? "N/A");
        }

        private static string? ExtractMigrationIdFromPath(PathString path)
        {
            // Extract migration ID from paths like: /api/v1/migrations/{migrationId}
            var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments?.Length >= 4 && segments[0] == "api" && segments[1] == "v1" && segments[2] == "migrations")
            {
                return segments[3];
            }
            return null;
        }

        private static string? ExtractFileNameFromPath(PathString path)
        {
            // Extract filename from paths like: /api/v1/migrations/{migrationId}/files/{filename}
            var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments?.Length >= 6 && segments[4] == "files")
            {
                return segments[5];
            }
            return null;
        }

        private static string ConvertFluentValidationErrors(
        IEnumerable<FluentValidation.Results.ValidationFailure> errors)
        {
            if (errors == null || !errors.Any())
            {
                return string.Empty;
            }

            // Group errors by PropertyName
            var groupedErrors = errors.GroupBy(
                failure => failure.PropertyName,
                (propertyName, failures) => new
                {
                    PropertyName = propertyName,
                    ErrorMessages = failures.Select(f => f.ErrorMessage).ToList()
                }
            );

            // Build the result string
            var resultBuilder = new System.Text.StringBuilder();

            foreach (var group in groupedErrors)
            {
                if (string.IsNullOrEmpty(group.PropertyName))
                {
                    // Handle general errors (not tied to a specific property)
                    resultBuilder.AppendLine("General Errors:");
                }
                else
                {
                    resultBuilder.AppendLine($"Property: {group.PropertyName}");
                }

                foreach (var errorMessage in group.ErrorMessages)
                {
                    resultBuilder.AppendLine($"- {errorMessage}");
                }
                resultBuilder.AppendLine(); // Add an empty line for separation between properties
            }

            return resultBuilder.ToString().TrimEnd(); // Trim any trailing newlines
        }
    }

    // Extension method to register the middleware
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
