using SQLMigrationAssistant.API;
using SQLMigrationAssistant.API.Middleware;
using SQLMigrationAssistant.Application;
using SQLMigrationAssistant.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
        .AddPresentationServices()
        .AddApplicationServices()
        .AddInfrastructureServices(builder.Configuration)
        .AddControllers();

// Cloud Run automatically provides PORT environment variable
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    // Only if the variable PORT exists (as in Cloud Run)
    builder.WebHost.UseUrls($"http://*:{port}");
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

//Enable CORS
app.UseCors("CorsPolicy");

app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthorization();

app.MapControllers();

app.Run();
