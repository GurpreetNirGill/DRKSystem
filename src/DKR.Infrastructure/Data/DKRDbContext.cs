using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Shared.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DKR.Infrastructure.Data;

public class DKRDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    private readonly IDeploymentService _deploymentService;
    private readonly string? _tenantId;

    public DKRDbContext(
        DbContextOptions<DKRDbContext> options,
        IConfiguration configuration,
        IDeploymentService deploymentService,
        IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _configuration = configuration;
        _deploymentService = deploymentService;

        // Tenant-ID aus HttpContext extrahieren (nur im Cloud-Modus)
        if (_deploymentService.IsCloudDeployment() && httpContextAccessor?.HttpContext != null)
        {
            _tenantId = httpContextAccessor.HttpContext.Items["TenantId"]?.ToString();
        }
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<EmergencyEvent> EmergencyEvents { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<HarmReduction> HarmReductions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Supply> Supply { get; set; }
    public DbSet<SessionLog> SessionLog { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var deploymentMode = _deploymentService.DetectMode();

            switch (deploymentMode)
            {
                case Core.Interfaces.DeploymentMode.Cloud:
                    var cloudConnection = _configuration.GetConnectionString("Cloud:Primary");
                    if (!string.IsNullOrEmpty(_tenantId))
                    {
                        cloudConnection = cloudConnection?.Replace("{TenantId}", _tenantId);
                    }
                    optionsBuilder.UseSqlServer(cloudConnection);
                    break;

                case Core.Interfaces.DeploymentMode.OnPremise:
                    var onPremiseConnection = _configuration.GetConnectionString("OnPremise:SqlServer");
                    optionsBuilder.UseSqlServer(onPremiseConnection);
                    break;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global Query Filter für Multi-Tenancy (nur im Cloud-Modus)
        if (_deploymentService.IsCloudDeployment() && !string.IsNullOrEmpty(_tenantId))
        {
            modelBuilder.Entity<Client>().HasQueryFilter(c => c.TenantId == _tenantId);
            modelBuilder.Entity<Session>().HasQueryFilter(s => s.TenantId == _tenantId);
            modelBuilder.Entity<EmergencyEvent>().HasQueryFilter(e => e.TenantId == _tenantId);
            modelBuilder.Entity<HarmReduction>().HasQueryFilter(h => h.TenantId == _tenantId);
        }

        // Client Configuration
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UUID).IsUnique();
            entity.Property(e => e.UUID).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Gender).HasConversion<string>();
            entity.Property(e => e.MainSubstance).HasConversion<string>();
            entity.Property(e => e.TreatmentHistory).HasConversion<string>();
        });

        // Session Configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Substance).HasConversion<string>();
            entity.Property(e => e.ApplicationMethod).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasOne(e => e.Client)
                .WithMany(c => c.Sessions)
                .HasForeignKey(e => e.ClientId);
        });

        // Audit Log Configuration (WORM)
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // Keine automatische Generierung
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Hash).IsRequired();
            entity.HasIndex(e => e.Hash).IsUnique();

            // Keine Update oder Delete erlaubt (WORM)
            // Configure WORM behavior at entity level instead of using deprecated APIs
        });

        // Supply Configuration
        modelBuilder.Entity<Supply>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Session)
                .WithOne(c => c.Supply)
                .HasForeignKey<Supply>(x => x.SessionId); // ✅ Specify the dependent type explicitly
        });

        // SessionLog Configuration
        modelBuilder.Entity<SessionLog>(entity =>
        {
            entity.Property(e => e.InitialStatus).HasConversion<string>();
            entity.Property(e => e.FinalStatus).HasConversion<string>();
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Session)
                .WithMany(c => c.SessionLogs)
                .HasForeignKey(x => x.SessionId); // ✅ Specify the dependent type explicitly
        });


        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

       
    }
}