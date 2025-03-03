// Program.cs (for .NET 6+)
using BlockedCountriesApi.BackgroundServices;
using BlockedCountriesApi.Repositories;
using BlockedCountriesApi.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Blocked Countries API",
        Version = "v1",
        Description = "API for managing blocked countries and validating IP addresses"
    });
});

// Register HttpClient for the geolocation service
builder.Services.AddHttpClient<IGeoLocationService, IpapiGeoLocationService>();

// Register repositories as singletons for in-memory storage
builder.Services.AddSingleton<IBlockedCountriesRepository, BlockedCountriesRepository>();
builder.Services.AddSingleton<ILogRepository, LogRepository>();

// Register the geolocation service
builder.Services.AddScoped<IGeoLocationService, IpapiGeoLocationService>();

// Register background service
builder.Services.AddHostedService<TemporalBlocksCleanupService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blocked Countries API V1");
        // Set Swagger UI at the app's root
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Configure the default launch URL in all environments
app.Run();