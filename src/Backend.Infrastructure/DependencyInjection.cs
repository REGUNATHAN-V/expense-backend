using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Backend.Application.Interfaces.Repositories;
using Backend.Application.Interfaces.Services;
using Backend.Application.Services;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;

namespace Backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing.");

        // services.AddDbContext<AppDbContext>(options =>
        //     options.UseMySql(
        //         connectionString,
        //         ServerVersion.AutoDetect(connectionString)));

        services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0))));

        services.AddHttpContextAccessor();
        
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IMakerService, MakerService>();
        services.AddScoped<IApproverService, ApproverService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IReportService, ReportService>();
                




        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
