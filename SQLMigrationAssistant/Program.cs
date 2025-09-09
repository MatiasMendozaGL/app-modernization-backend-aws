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

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port) && int.TryParse(port, out int portNumber))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{portNumber}");
}
else
{
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
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
