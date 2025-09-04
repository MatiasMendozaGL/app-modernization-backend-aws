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

builder.WebHost.UseUrls($"http://*:80");

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
