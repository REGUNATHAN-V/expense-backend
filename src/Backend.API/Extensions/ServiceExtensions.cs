
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Backend.Application;
using Backend.Infrastructure;
using Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Common.Responses;

namespace Backend.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Application and Infrastructure
        // services.AddApplication();
        services.AddInfrastructure(configuration);

        // Controllers and Swagger
        // services.AddControllers();
        services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage);

                var message = string.Join("; ", errors);

                var response =
                    ApiErrorResponse.FailureResponse(message);

                return new BadRequestObjectResult(response);
            };
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // CORS
        var allowedOrigins =
            configuration.GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
            });
        });

        // JWT Authentication
        var jwt = configuration.GetSection("Jwt");

        var issuer = jwt["Issuer"]
            ?? throw new InvalidOperationException(
                "Jwt:Issuer is missing.");

        var audience = jwt["Audience"]
            ?? throw new InvalidOperationException(
                "Jwt:Audience is missing.");

        var key = jwt["Key"]
            ?? throw new InvalidOperationException(
                "Jwt:Key is missing.");

        services.AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,

                        ValidateAudience = true,
                        ValidAudience = audience,

                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(key)),

                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
            });

        services.AddAuthorization();

        // Rate limiting: 100 requests per minute per IP
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            options.AddPolicy("IpRateLimit", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        httpContext.Connection
                            .RemoteIpAddress?.ToString()
                        ?? "unknown",

                    factory: _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            AutoReplenishment = true,
                            QueueLimit = 0
                        }));
        });

        // Health checks
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("mysql");

        return services;
    }
}
