
using Backend.API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((context, logger) =>
{
    logger
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/production-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14);
});

// Register application services
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Request logging
app.UseSerilogRequestLogging();

// CORS
app.UseCors("DefaultCors");

// Rate limiting
app.UseRateLimiter();

// Authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Integration testing
public partial class Program { }
