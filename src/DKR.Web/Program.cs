using DKR.Core.Interfaces;
using DKR.Infrastructure.Configuration;
using DKR.Infrastructure.Extensions;
using DKR.Infrastructure.Data;
using DKR.Infrastructure.Middleware;
using DKR.Web.Hubs;
using DKR.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Serilog;
using DKR.Core.Services;
using DKR.Infrastructure.Services;
using DKR.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using DKR.Web.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using DKR.Web.Pages;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using DKR.Core.Entities;
using Microsoft.AspNetCore.Components.Server;
using System;
using Azure.Core;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

// Serilog konfigurieren
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Deployment-Modus erkennen
var deploymentService = new DeploymentDetectionService(builder.Configuration);
var deploymentMode = deploymentService.DetectMode();

Log.Information("Starting DKR System in {DeploymentMode} mode", deploymentMode);

// Services hinzufügen
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddServerSideBlazor();
// Hybrid-Konfiguration
builder.Services.AddSingleton<IDeploymentService>(deploymentService);
builder.Services.ConfigureHybridServices(deploymentMode, builder.Configuration);
// Repository Services
builder.Services.AddScoped<IEmergencyRepository, EmergencyRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IHarmReductionRepository, HarmReductionRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<ISupplyRepository, SupplyRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISessionLogRepository, SessionLogRepository>();
// Register UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register SecurityService
builder.Services.AddScoped<SecurityService>();

// Register application services
builder.Services.AddScoped<EmergencyService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<HarmReductionService>();
builder.Services.AddScoped<KDSExportService>();
builder.Services.AddScoped<TDIExportService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<SessionLogService>();

// Register IEmailService with its implementation
builder.Services.AddScoped<IEmailService, EmailService>();


// Register IAuditService with its implementation
builder.Services.AddScoped<IAuditService, AuditService>();

// Register ISMSService with its implementation
builder.Services.AddScoped<ISMSService, SmsService>();

// Register ICurrentUserService with its implementation
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Register IWhatsAppService with its implementation
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();

// Register IAuthorizationService with its implementation
builder.Services.AddScoped<DKR.Core.Interfaces.IAuthorizationService, AuthorizationService>();

// Register HttpClient for TDIExportService
builder.Services.AddHttpClient<TDIExportService>();

// Register IIdentityProvider with DummyIdentityProvider implementation
builder.Services.AddScoped<IIdentityProvider, RealIdentityProvider>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
// Register DbContext
builder.Services.AddDbContext<DKRDbContext>(options =>
{
    var connectionString = deploymentMode == DKR.Core.Interfaces.DeploymentMode.Cloud
        ? builder.Configuration.GetConnectionString("Cloud:Primary")
        : builder.Configuration.GetConnectionString("OnPremise:SqlServer");
    options.UseSqlServer(connectionString);
});

//// Add authentication services
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/login";   // redirect unauthenticated users

//        options.LogoutPath = "/logout"; // optional

//        options.AccessDeniedPath = "/access-denied"; // optional

//        options.SlidingExpiration = true;

//        options.ExpireTimeSpan = TimeSpan.FromHours(1);
//    });

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddAuthorization();
// Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register custom AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>(); builder.Services.AddScoped<AuthenticationStateProvider, InMemoryAuthStateProvider>();
// CORS für API
builder.Services.AddCors(options =>
{
    options.AddPolicy("DKRPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Multi-Tenant Middleware (nur im Cloud-Modus)
if (deploymentMode == DKR.Core.Interfaces.DeploymentMode.Cloud)
{
    app.UseMiddleware<TenantMiddleware>();
}

app.UseCors("DKRPolicy");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapHub<DashboardHub>("/dashboardhub");
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
// Seed the database
SeedData.Initialize(app.Services);

try
{
    Log.Information("DKR System successfully started");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "DKR System terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}