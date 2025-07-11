using DKR.Core.Interfaces;
using DKR.Core.Services;
using DKR.Infrastructure.Configuration;
using DKR.Infrastructure.Data;
using DKR.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DKR.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureHybridServices(
        this IServiceCollection services,
        DeploymentMode deploymentMode,
        IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<DKRDbContext>(options =>
        {
            var connectionString = deploymentMode == DeploymentMode.Cloud
                 ? configuration.GetConnectionString("Cloud:Primary")
               : configuration.GetConnectionString("OnPremise:SqlServer");
            options.UseSqlServer(connectionString);
        });

        // Core Services
        services.AddScoped<ClientService>();
        services.AddScoped<SessionService>();
        services.AddScoped<EmergencyService>();
        services.AddScoped<SecurityService>();
        services.AddScoped<AuditService>();
        services.AddScoped<KDSExportService>();
        services.AddScoped<TDIExportService>();

        // Repositories
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();

        // Infrastructure Services
        services.AddSingleton<IDeploymentService, DeploymentDetectionService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<INotificationService, DKR.Infrastructure.Services.NotificationService>();

        switch (deploymentMode)
        {
            case DeploymentMode.Cloud:
                services.ConfigureCloudServices(configuration);
                break;
            case DeploymentMode.OnPremise:
                services.ConfigureOnPremiseServices(configuration);
                break;
            case DeploymentMode.Hybrid:
                // Hybrid lädt beide Implementierungen und entscheidet zur Laufzeit
                services.ConfigureCloudServices(configuration);
                services.ConfigureOnPremiseServices(configuration);
                break;
        }

        return services;
    }

    private static IServiceCollection ConfigureCloudServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Cloud-spezifische Services werden zur Laufzeit aus DKR.Infrastructure.Cloud geladen
        // Placeholder für dynamisches Laden
        return services;
    }

    private static IServiceCollection ConfigureOnPremiseServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // On-Premise-spezifische Services werden zur Laufzeit aus DKR.Infrastructure.OnPremise geladen
        // Placeholder für dynamisches Laden
        return services;
    }
}