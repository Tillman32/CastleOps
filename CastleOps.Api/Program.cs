using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using CastleOps.Api.Infrastructure.Cache;
using CastleOps.Api.Infrastructure.Database;
using CastleOps.Api.Infrastructure.Database.Repository;
using CastleOps.Api.Services;
using CastleOps.Core.HTTP.Clients;
using Microsoft.EntityFrameworkCore;
using Serilog;

// On Linux (Docker), use /app/data and /app/logs so docker-compose volume mounts take effect.
// On Windows/macOS, fall back to the local AppData folder.
var dataDir = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
    ? "/app/data"
    : Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CastleOps");
var logDir = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
    ? "/app/logs"
    : dataDir;

Directory.CreateDirectory(dataDir);
Directory.CreateDirectory(logDir);

// Configure Serilog before building the host
var latestLogPath = Path.Join(logDir, "castle-api-log-latest.txt");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    // Daily rolling log files: castle-api-log-20240915.txt, etc.
    .WriteTo.File(Path.Join(logDir, "castle-api-log-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14)
    // Always-updated 'latest' log (single file)
    .WriteTo.File(latestLogPath,
        shared: true,
        retainedFileCountLimit: 1) // no rolling here; keeps one file
    .CreateLogger();

try
{
    Log.Information("Starting up the application");

    var builder = WebApplication.CreateBuilder(args);

    // Logging
    builder.Host.UseSerilog();

    // Cache
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton(typeof(MemCache<>));

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();
    builder.Services.AddControllers();

    // Database
    Log.Information("Configuring database...");
    var dbPath = Path.Join(dataDir, "app.db");
    builder.Services.AddDbContext<DatabaseContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));

    builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

    // Services
    Log.Information("Configuring services...");
    builder.Services.AddScoped<DeviceService>();
    builder.Services.AddScoped<MarketplaceService>();
    builder.Services.AddScoped<PeonService>();
    builder.Services.AddScoped<ClientService>();

    // Clients
    Log.Information("Configuring clients...");
    builder.Services.AddHttpClient<GitHubClient>(client =>
    {
        client.BaseAddress = new Uri("https://api.github.com/");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
    });

    // Add CORS services
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowBlazorWasm", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            db.Database.EnsureCreated();
        }
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    // Add CORS middleware
    app.UseCors("AllowBlazorWasm");

    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
