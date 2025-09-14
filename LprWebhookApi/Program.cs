using Serilog;
using Serilog.Formatting.Compact;
using Microsoft.EntityFrameworkCore;
using LprWebhookApi.Data;
using LprWebhookApi.Services;
using LprWebhookApi.Middleware;


// Configure Serilog with both JSON and human-readable formats
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    // Console: wrap message in optional color tokens if present
    .WriteTo.Console(outputTemplate: "{ColorStart}[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{ColorReset}{NewLine}{Exception}")
    // Human rolling file
    .WriteTo.File("logs/lpr-webhook-human-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    // Structured JSON rolling file
    .WriteTo.File(new CompactJsonFormatter(), "logs/lpr-webhook-json-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on all IP addresses (HTTP only for development)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5174); // HTTP only
    // Disable HTTPS for development to avoid certificate issues
    // options.ListenAnyIP(7214, listenOptions =>
    // {
    //     listenOptions.UseHttps(); // HTTPS
    // });
});

// Use Serilog for logging
builder.Host.UseSerilog();

// Add Entity Framework with PostgreSQL
builder.Services.AddDbContext<LprDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=lpr_webhook;Username=postgres;Password=postgres"));

// Add services
builder.Services.AddScoped<WhitelistSyncService>();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add MVC services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Disable HTTPS redirection for development to avoid certificate issues
// app.UseHttpsRedirection();

// Enable CORS
app.UseCors();

// Custom request/response logging with markers and console colors
app.UseRequestResponseLogging();

// Configure MVC routing
app.MapControllers();

app.Run();
