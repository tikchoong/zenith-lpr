using Serilog;
using Serilog.Formatting.Compact;
using Microsoft.EntityFrameworkCore;
using LprWebhookApi.Data;

// Configure Serilog with both JSON and human-readable formats
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/lpr-webhook-human-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 3,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(new CompactJsonFormatter(), "logs/lpr-webhook-json-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 3)
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

// Add Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
    };
});

// Configure MVC routing
app.MapControllers();

app.Run();
