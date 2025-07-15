# DKR Management System - Blazor Entwicklungs-Roadmap

## 🏗️ Phase 1: Hybrid-Architektur Setup (Woche 1-2)

### 1.1 Deployment-Agnostisches Blazor Projekt
- [ ] .NET 8 Blazor Server Projekt mit Hybrid-Support erstellen
- [ ] Solution Structure für Cloud + On-Premise:
  - `DKR.Web` (Blazor Server UI - deployment-agnostic)
  - `DKR.Core` (Domain Models & Business Logic) 
  - `DKR.Infrastructure` (Abstraction Layer für Storage/Identity)
  - `DKR.Infrastructure.Cloud` (Azure/AWS Services)
  - `DKR.Infrastructure.OnPremise` (Local Services)
  - `DKR.Shared` (DTOs, Enums, Constants)
  - `DKR.Deployment` (Docker, Helm Charts, ARM Templates)
- [ ] NuGet Packages für Hybrid-Deployment:
  - `Microsoft.EntityFrameworkCore.SqlServer` (On-Premise)
  - `Microsoft.EntityFrameworkCore.Cosmos` (Cloud Option)
  - `Azure.Storage.Blobs` (Cloud Files)
  - `Microsoft.Extensions.Hosting` (Background Services)
  - `Docker.DotNet` (Container Management)
  - `AutoMapper`, `FluentValidation`, `Serilog.AspNetCore`

### 1.2 Hybrid Database & Storage Abstraction
- [ ] IRepository Pattern für Database-Abstraktion:
  - `IClientRepository`, `ISessionRepository`, `IInventoryRepository`
  - SQL Server Implementation (On-Premise)
  - Azure SQL / Cosmos DB Implementation (Cloud)
- [ ] IFileStorage Abstraction:
  - `LocalFileStorage` für On-Premise
  - `AzureBlobStorage` für Cloud
  - `AWSS3Storage` für AWS Cloud
- [ ] Entity Models mit Hybrid-Support:
  - `Client` (UUID, Tenant-ID, Encryption-ready)
  - `Session` (Multi-tenant, Cloud/Local timestamps)
  - `EmergencyEvent` (Notification abstraction)
  - `InventoryItem` (Multi-location tracking)
  - `HarmReductionService` (Location-agnostic)
  - `User` (Identity abstraction, 2FA)
  - `AuditLog` (WORM-compatible, Blockchain-ready)
  - `Tenant` (Cloud Multi-tenancy + On-Premise single tenant)
- [ ] Configuration-driven DbContext:
  ```csharp
  public class HybridDbContext : DbContext
  {
      protected override void OnConfiguring(DbContextOptionsBuilder options)
      {
          var deploymentMode = Configuration["Deployment:Mode"];
          if (deploymentMode == "Cloud")
              options.UseSqlServer(cloudConnectionString);
          else
              options.UseSqlServer(localConnectionString);
      }
  }
  ```

### 1.3 Hybrid Authentication & Identity
- [ ] IIdentityProvider Abstraction:
  - `LocalIdentityProvider` (ASP.NET Core Identity)
  - `AzureADProvider` (Azure AD B2C)
  - `AWSCognitoProvider` (AWS Cognito)
- [ ] Multi-Tenant User Model:
  ```csharp
  public class ApplicationUser : IdentityUser
  {
      public string TenantId { get; set; }  // For Cloud Multi-tenancy
      public string FacilityId { get; set; } // For On-Premise
      public DeploymentMode Mode { get; set; }
  }
  ```
- [ ] Rollenbasierte Autorisierung (deployment-agnostic):
  - `Administrator`, `Fachkraft`, `Sozialarbeiter`, `ReadOnly`
  - `TenantAdmin` (nur Cloud), `SystemAdmin` (nur On-Premise)
- [ ] 2FA Abstraction:
  - Local TOTP (On-Premise)
  - Azure AD MFA (Cloud)
  - SMS/Email Fallback (beide)
- [ ] JWT Token Handling:
  - Cloud: Azure AD Tokens
  - On-Premise: Local JWT mit Custom Claims

## 🧩 Phase 2: Core Components & Services (Woche 3-4)

### 2.1 Blazor Layout & Navigation
- [ ] MainLayout mit Sidebar-Navigation
- [ ] Responsive Header mit User-Info
- [ ] NavMenu Component mit rollenbasierter Sichtbarkeit
- [ ] Breadcrumb Navigation
- [ ] Loading States & Progress Indicators
- [ ] Toast Notification System

### 2.2 Client Management System
- [ ] `ClientService` für CRUD-Operationen
- [ ] UUID-Generator (KL-YYYY-NNNN Format)
- [ ] Dublettencheck Algorithm
- [ ] `ClientListComponent` mit Filterung/Sortierung
- [ ] `ClientDetailComponent` für Check-in
- [ ] KDS 3.0 Mapping & Validation
- [ ] GDPR-konforme Archivierung nach 10 Jahren

### 2.3 Session Management
- [ ] `SessionService` mit Real-time Updates
- [ ] `SessionTimerComponent` (SignalR für Live-Updates)
- [ ] Session State Machine (Wartend → Aktiv → Beendet)
- [ ] Maximum Session Duration Enforcement (30 Min)
- [ ] Multi-Room Management
- [ ] Automatic Cleanup bei Timeout

### 2.4 Barcode/QR-Code Integration
- [ ] `BarcodeReaderComponent` mit WebAssembly
- [ ] Camera Access für Mobile Devices
- [ ] QR-Code Generator für Client-IDs
- [ ] NFC-Tag Support (Mobile)
- [ ] Fallback: Manuelle Eingabe

## 🚨 Phase 3: Emergency & Compliance (Woche 5-6)

### 3.1 Notfall-System
- [ ] `EmergencyComponent` mit One-Click-Alert
- [ ] ICD-10 Code Mapping
- [ ] Automatic Notifications:
  - E-Mail an Behörden
  - SMS an Rettungsdienst
  - WhatsApp an Leitungsteam
- [ ] Eskalationsmatrix konfigurierbar
- [ ] Naloxon-Verabreichung Protokoll

### 3.2 Audit Trail & WORM Logging
- [ ] `AuditService` mit unveränderlichen Logs
- [ ] Blockchain-basierte Integrität (Hash-Chain)
- [ ] Tamper-evident Storage
- [ ] Export-Funktionen für Compliance
- [ ] GDPR Article 32 Compliance

### 3.3 Security Features
- [ ] TLS 1.3 Enforcement
- [ ] AES-256 Field-Level Encryption
- [ ] Input Validation & Sanitization
- [ ] Rate Limiting & DDoS Protection
- [ ] Security Headers (HSTS, CSP, etc.)
- [ ] Penetration Test Integration Points

## 📊 Phase 4: Reporting & Export (Woche 7-8)

### 4.1 KDS 3.0 Export Engine
- [ ] `KDSExportService` mit XML-Generierung
- [ ] Automatische Validierung gegen KDS-Schema
- [ ] Batch Processing für große Datenmengen
- [ ] Quartal/Jahr-Export-Automation
- [ ] Deutsche Suchthilfestatistik Integration

### 4.2 TDI 3.0 EMCDDA Integration
- [ ] `TDIExportService` für European Drug Report
- [ ] REITOX Focal Point API Integration
- [ ] CSV/JSON Format Support
- [ ] Automatic HTTPS Upload
- [ ] Retry Logic & Error Handling

### 4.3 Dashboard & Analytics
- [ ] Real-time KPI Dashboard (SignalR)
- [ ] Chart.js Integration für Visualisierungen
- [ ] Custom Report Builder
- [ ] PDF Report Generation
- [ ] Substanz-Trend-Analyse
- [ ] Präventions-Erfolgs-Metriken

## 🩺 Phase 5: Harm Reduction Module (Woche 9-10)

### 5.1 Terminverwaltung
- [ ] `AppointmentService` für HIV/HCV-Tests
- [ ] Calendar Integration (iCal, Google Calendar)
- [ ] Push Notification System:
  - Web Push API
  - Mobile App Notifications
  - SMS/E-Mail Fallback
- [ ] Reminder Automation (24h, 2h vor Termin)

### 5.2 Medizinische Services
- [ ] Wundversorgung-Dokumentation
- [ ] Venenberatung-Protokolle
- [ ] Substitutions-Software-Integration (PSS)
- [ ] Test-Ergebnis-Management
- [ ] Nachsorge-Workflows

### 5.3 Inventory Management
- [ ] `InventoryService` mit Chargen-Tracking
- [ ] Automatic Reorder Points
- [ ] Ablaufdatum-Monitoring
- [ ] FIFO/FEFO Logic Implementation
- [ ] Supplier Integration (EDI/API)
- [ ] Kostenstellen-Zuordnung

## 📱 Phase 6: Mobile & Offline (Woche 11-12)

### 6.1 Blazor Hybrid Mobile App
- [ ] .NET MAUI Blazor Hybrid Project
- [ ] Shared Razor Components zwischen Web/Mobile
- [ ] SQLite für Offline-Storage
- [ ] Background Sync Service
- [ ] Conflict Resolution Algorithm

### 6.2 Offline-First Architecture
- [ ] Service Worker für PWA
- [ ] IndexedDB für Browser-Storage
- [ ] Delta Sync Implementation
- [ ] Network Detection & Queuing
- [ ] Optimistic UI Updates

### 6.3 Tablet Integration
- [ ] Touch-optimierte UI Components
- [ ] Gesture Support
- [ ] Barcode Scanner Integration
- [ ] Multiple Device Sync
- [ ] Cross-Device Session Handover

## 🔗 Phase 7: API & Interoperability (Woche 13-14)

### 7.1 REST API Development
- [ ] ASP.NET Core Web API Project
- [ ] OpenAPI/Swagger Documentation
- [ ] Versioning Strategy (Header-based)
- [ ] Rate Limiting & Throttling
- [ ] API Key Management

### 7.2 FHIR R4 Integration
- [ ] FHIR Server Implementation (Firely)
- [ ] Patient Resource Mapping
- [ ] Observation Resource für Konsumvorgänge
- [ ] Condition Resource für Diagnosen
- [ ] Bundle Creation für Exports

### 7.3 HL7 Integration
- [ ] HL7 v2.x Message Processing
- [ ] ADT (Admission/Discharge/Transfer) Messages
- [ ] Hospital System Integration (KIS)
- [ ] MLLP Protocol Implementation
- [ ] Message Routing & Transformation

## 🌍 Phase 8: Multi-Tenancy & Internationalization (Woche 15-16)

### 8.1 Multi-Tenant Architecture (Cloud)
- [ ] Tenant Resolution Strategy:
  ```csharp
  public class TenantMiddleware
  {
      public async Task InvokeAsync(HttpContext context)
      {
          // Subdomain: hamburg.dkr-system.com
          var tenant = ExtractTenantFromSubdomain(context.Request.Host);
          
          // Header: X-Tenant-ID (für API calls)
          if (string.IsNullOrEmpty(tenant))
              tenant = context.Request.Headers["X-Tenant-ID"];
              
          context.Items["TenantId"] = tenant;
      }
  }
  ```
- [ ] **Database Strategies:**
  - **Option A:** Database per Tenant (höchste Isolation)
  - **Option B:** Shared Database + Tenant-ID (kosteneffizienter)
  - **Option C:** Hybrid (kritische Daten getrennt)
- [ ] **Tenant Configuration Management:**
  ```csharp
  public class TenantConfiguration
  {
      public string TenantId { get; set; }
      public string Name { get; set; }
      public string[] AllowedDomains { get; set; }
      public Dictionary<string, object> Features { get; set; }
      public LocalizationSettings Localization { get; set; }
      public ComplianceSettings Compliance { get; set; }
  }
  ```
- [ ] **Data Isolation & Security:**
  ```csharp
  // Global Query Filter für automatische Tenant-Isolation
  builder.Entity<Client>()
      .HasQueryFilter(c => c.TenantId == _tenantService.GetCurrentTenant());
  ```
- [ ] **Custom Branding per Tenant:**
  - Logo Upload & Management
  - CSS Theme Customization
  - White-Label Options

### 8.2 Single-Tenant Architecture (On-Premise)
- [ ] **Facility-based Organization:**
  ```csharp
  public class Facility
  {
      public string Id { get; set; }
      public string Name { get; set; }
      public string Address { get; set; }
      public string LicenseNumber { get; set; }
      public FacilityType Type { get; set; } // DKR, Substitution, Combined
      public int Capacity { get; set; }
  }
  ```
- [ ] **Local Configuration:**
  - Single facility setup
  - Local user management
  - On-premise backup strategies
  - Local compliance reporting

### 8.3 Internationalization (Both Deployments)
- [ ] **Resource Files für 9 Sprachen:**
  ```
  Resources/
  ├── DKR.de.resx          # Deutsch (Standard)
  ├── DKR.en.resx          # English
  ├── DKR.fr.resx          # Français  
  ├── DKR.es.resx          # Español
  ├── DKR.it.resx          # Italiano
  ├── DKR.nl.resx          # Nederlands
  ├── DKR.pt.resx          # Português
  ├── DKR.pl.resx          # Polski
  └── DKR.cs.resx          # Čeština
  ```
- [ ] **Culture-specific Features:**
  ```csharp
  public class LocalizationService
  {
      public string FormatCurrency(decimal amount, string culture)
      {
          return culture switch
          {
              "de-DE" => amount.ToString("C", new CultureInfo("de-DE")), // €
              "en-GB" => amount.ToString("C", new CultureInfo("en-GB")), // £
              "ch-CH" => amount.ToString("C", new CultureInfo("de-CH")), // CHF
              _ => amount.ToString("C", CultureInfo.CurrentCulture)
          };
      }
  }
  ```
- [ ] **Country-specific Compliance:**
  ```csharp
  public class ComplianceRules
  {
      public static Dictionary<string, ComplianceSettings> CountryRules = new()
      {
          ["DE"] = new() { DataRetention = TimeSpan.FromDays(3653), RequiredFields = ["Gender", "BirthYear"] },
          ["AT"] = new() { DataRetention = TimeSpan.FromDays(3653), RequiredFields = ["Gender", "BirthYear"] },
          ["CH"] = new() { DataRetention = TimeSpan.FromDays(3653), RequiredFields = ["Gender", "Region"] },
          ["NL"] = new() { DataRetention = TimeSpan.FromDays(2555), RequiredFields = ["Gender", "PostalCode"] }
      };
  }
  ```

## 📦 Phase 11: Deployment & DevOps (Woche 19-20)

### 11.1 Container-based Deployment
- [ ] Multi-Stage Dockerfile für Hybrid-Deployment:
  ```dockerfile
  # Build stage
  FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
  
  # Cloud runtime
  FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS cloud-runtime
  ENV DEPLOYMENT_MODE=Cloud
  
  # On-Premise runtime  
  FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS onpremise-runtime
  ENV DEPLOYMENT_MODE=OnPremise
  ```
- [ ] Docker Compose für On-Premise:
  - Blazor App Container
  - SQL Server Container
  - Redis Container
  - Reverse Proxy (nginx/Traefik)
- [ ] Kubernetes Manifests für Cloud:
  - Deployment, Service, Ingress
  - ConfigMaps für Environment-spezifische Settings
  - Secrets für Connection Strings

### 11.2 Cloud Deployment (Azure & AWS)
- [ ] **Azure Deployment:**
  - ARM/Bicep Templates
  - Azure App Service für Blazor
  - Azure SQL Database
  - Azure Blob Storage
  - Azure Key Vault für Secrets
  - Application Insights
- [ ] **AWS Deployment:**
  - CloudFormation Templates
  - AWS ECS/Fargate für Container
  - AWS RDS für Database
  - AWS S3 für File Storage
  - AWS Secrets Manager
  - CloudWatch Monitoring
- [ ] Multi-Region Setup:
  - EU-West (Frankfurt) - DACH Region
  - EU-Central (Amsterdam) - BeNeLux
  - EU-South (Madrid) - Iberia

### 11.3 On-Premise Deployment
- [ ] **Installer Package:**
  - Windows MSI Installer
  - Linux .deb/.rpm Packages
  - macOS .pkg Installer
- [ ] **Automated Setup Script:**
  ```powershell
  # Windows PowerShell
  Install-DKRSystem -Mode OnPremise -DatabaseServer localhost
  ```
  ```bash
  # Linux Bash
  curl -sSL https://install.dkr-system.com | bash -s -- --mode=onpremise
  ```
- [ ] **Configuration Wizard:**
  - Web-based Setup (Port 8080)
  - Database Connection Testing
  - SSL Certificate Configuration
  - Initial Admin User Creation
- [ ] **System Requirements Check:**
  - .NET 8 Runtime
  - SQL Server 2019+ / PostgreSQL 13+
  - Minimum Hardware Specifications
  - Network Port Requirements

### 11.4 Hybrid Configuration Management
- [ ] **Environment Detection:**
  ```csharp
  public enum DeploymentMode 
  { 
      Cloud, 
      OnPremise, 
      Hybrid 
  }
  
  public class DeploymentService : IDeploymentService
  {
      public DeploymentMode DetectMode()
      {
          // Auto-detect based on environment variables
          if (Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") != null)
              return DeploymentMode.Cloud;
          return DeploymentMode.OnPremise;
      }
  }
  ```
- [ ] **Dynamic Service Registration:**
  ```csharp
  public void ConfigureServices(IServiceCollection services)
  {
      var mode = services.GetRequiredService<IDeploymentService>().DetectMode();
      
      switch (mode)
      {
          case DeploymentMode.Cloud:
              services.AddScoped<IFileStorage, AzureBlobStorage>();
              services.AddScoped<IIdentityProvider, AzureADProvider>();
              services.AddScoped<INotificationService, AzureServiceBus>();
              break;
          case DeploymentMode.OnPremise:
              services.AddScoped<IFileStorage, LocalFileStorage>();
              services.AddScoped<IIdentityProvider, LocalIdentityProvider>();
              services.AddScoped<INotificationService, SMTPNotificationService>();
              break;
      }
  }
  ```
- [ ] **Feature Toggles per Deployment:**
  ```json
  {
    "Features": {
      "Cloud": {
        "MultiTenant": true,
        "AutoScaling": true,
        "GlobalSearch": true
      },
      "OnPremise": {
        "MultiTenant": false,
        "AdvancedSecurity": true,
        "LocalBackup": true
      }
    }
  }
  ```

## 🧪 Phase 10: Testing & Quality Assurance (Woche 19-20)

### 10.1 Unit Testing
- [ ] xUnit Test Project Setup
- [ ] Repository Pattern Testing
- [ ] Service Layer Unit Tests
- [ ] Mock Dependencies (Moq)
- [ ] Code Coverage Analysis

### 10.2 Integration Testing
- [ ] TestServer für API Testing
- [ ] Database Integration Tests
- [ ] External API Mocking
- [ ] End-to-End Scenarios
- [ ] Performance Testing

### 10.3 Security Testing
- [ ] OWASP ZAP Integration
- [ ] Dependency Vulnerability Scanning
- [ ] Penetration Testing Automation
- [ ] Compliance Testing (GDPR, ISO 27001)

---

## 📋 Technische Spezifikationen für Claude Code

### Hybrid-Specific Development Environment Setup
```bash
# .NET 8 SDK Installation
dotnet --version  # Should be 8.0+

# Hybrid Project Creation Commands
dotnet new blazorserver -n DKR.Web
dotnet new classlib -n DKR.Core
dotnet new classlib -n DKR.Infrastructure
dotnet new classlib -n DKR.Infrastructure.Cloud
dotnet new classlib -n DKR.Infrastructure.OnPremise
dotnet new classlib -n DKR.Shared
dotnet new classlib -n DKR.Deployment

# Solution Setup
dotnet new sln -n DKRSystem
dotnet sln add **/*.csproj
```

### Key NuGet Packages for Hybrid Architecture
```xml
<!-- Shared Packages -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.0" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />

<!-- Cloud-Specific -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Cosmos" Version="8.0.0" />
<PackageReference Include="Azure.Storage.Blobs" Version="12.19.1" />
<PackageReference Include="Azure.Identity" Version="1.10.4" />
<PackageReference Include="Microsoft.Graph" Version="5.42.0" />

<!-- On-Premise Specific -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<PackageReference Include="StackExchange.Redis" Version="2.6.122" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
```

### Hybrid Connection String Templates
```json
{
  "ConnectionStrings": {
    "Cloud": {
      "Primary": "Server=tcp:dkr-eu-west.database.windows.net;Database=DKR_{TenantId};Authentication=Active Directory Default;Encrypt=true;",
      "CosmosDB": "AccountEndpoint=https://dkr-cosmos.documents.azure.com:443/;AccountKey={Key};"
    },
    "OnPremise": {
      "SqlServer": "Server=(localdb)\\mssqllocaldb;Database=DKRSystem;Trusted_Connection=true;MultipleActiveResultSets=true;Encrypt=true;TrustServerCertificate=true",
      "PostgreSQL": "Host=localhost;Database=dkr_system;Username=dkr_user;Password={Password}",
      "SQLite": "Data Source=dkr_system.db"
    }
  },
  "Deployment": {
    "Mode": "Auto", // Auto, Cloud, OnPremise
    "TenantMode": "Auto", // Single, Multi
    "Region": "EU-West"
  }
}
```

### Hybrid Folder Structure
```
DKRSystem/
├── src/
│   ├── DKR.Web/                    # Blazor Server UI (Deployment-agnostic)
│   │   ├── Components/             # Reusable UI Components
│   │   ├── Pages/                 # Blazor Pages
│   │   ├── Services/              # UI Services
│   │   ├── Middleware/            # Tenant Resolution, etc.
│   │   └── wwwroot/               # Static Files
│   ├── DKR.Core/                  # Domain Logic (Pure .NET)
│   │   ├── Entities/              # Entity Models
│   │   ├── Services/              # Business Services  
│   │   ├── Interfaces/            # Abstraction Contracts
│   │   ├── Enums/                 # Enumerations
│   │   └── ValueObjects/          # DDD Value Objects
│   ├── DKR.Infrastructure/        # Shared Infrastructure
│   │   ├── Abstractions/          # IFileStorage, IIdentityProvider
│   │   ├── Data/                  # Base DbContext, Repositories
│   │   ├── Services/              # Cross-cutting Services
│   │   └── Extensions/            # DI Extensions
│   ├── DKR.Infrastructure.Cloud/  # Cloud-specific Services
│   │   ├── Azure/                 # Azure implementations
│   │   ├── AWS/                   # AWS implementations  
│   │   ├── Storage/               # Blob Storage, etc.
│   │   └── Identity/              # Cloud Identity Providers
│   ├── DKR.Infrastructure.OnPremise/ # On-Premise Services
│   │   ├── Storage/               # Local File Storage
│   │   ├── Identity/              # Local Identity
│   │   ├── Backup/                # Local Backup Services
│   │   └── Monitoring/            # Local Health Checks
│   ├── DKR.Shared/                # Common DTOs & Models
│   │   ├── DTOs/                  # Data Transfer Objects
│   │   ├── Constants/             # System Constants
│   │   ├── Enums/                 # Shared Enumerations
│   │   └── Extensions/            # Extension Methods
│   └── DKR.Deployment/            # Deployment Artifacts
│       ├── Docker/                # Dockerfiles
│       ├── Kubernetes/            # K8s Manifests
│       ├── Azure/                 # ARM/Bicep Templates
│       ├── AWS/                   # CloudFormation
│       └── OnPremise/             # Installers, Scripts
├── tests/
│   ├── DKR.UnitTests/
│   ├── DKR.IntegrationTests/
│   ├── DKR.Cloud.Tests/           # Cloud-specific Tests
│   ├── DKR.OnPremise.Tests/       # On-Premise Tests
│   └── DKR.PerformanceTests/
└── docs/
    ├── Deployment/                # Deployment Guides
    │   ├── Cloud-Setup.md
    │   ├── OnPremise-Setup.md
    │   └── Hybrid-Migration.md
    ├── Architecture/              # System Design
    └── Compliance/                # GDPR, KDS, TDI Docs
```

### Hybrid-Specific Implementation Priority
1. **Abstraction Layer First** (Woche 1-2)
   - Interface definitions für alle Services
   - Dependency Injection Setup
   - Configuration Management

2. **Core Business Logic** (Woche 3-4)  
   - Domain Models (deployment-agnostic)
   - Business Services (keine Infrastructure-Dependencies)

3. **Dual Infrastructure Implementation** (Woche 5-8)
   - Cloud Services (Azure/AWS)
   - On-Premise Services (Local)
   - Feature Parity Testing

4. **Deployment Automation** (Woche 9-10)
   - Container Orchestration
   - CI/CD für beide Modi
   - Migration Tools

5. **Advanced Features** (Woche 11+)
   - Multi-tenancy (Cloud)
   - Enterprise Features (On-Premise)
   - Hybrid Scenarios (Data Sync, etc.)

---

## 🎯 Hybrid-Specific Definition of Done

Jedes Feature ist erst "Done" wenn:
- [ ] **Cloud Implementation** funktioniert (Azure + AWS)
- [ ] **On-Premise Implementation** funktioniert (Windows + Linux)
- [ ] **Abstraction Interface** definiert und implementiert
- [ ] **Configuration Switch** zwischen Modi funktioniert
- [ ] **Unit Tests** für beide Deployment-Modi
- [ ] **Integration Tests** Cloud und On-Premise getrennt
- [ ] **Docker Container** läuft in beiden Modi
- [ ] **Documentation** für beide Deployment-Optionen
- [ ] **Migration Path** zwischen Cloud ↔ On-Premise definiert
- [ ] **Performance** in beiden Modi gemessen (<2s Response Time)
- [ ] **Security Review** für Cloud + On-Premise bestanden
- [ ] **Compliance** GDPR/KDS/TDI für beide Modi validiert

---

**Diese Hybrid-Roadmap ist Claude Code ready für internationale Skalierung!** 🌍🚀

## 💼 **Business Model Hybrid-Support:**

**Cloud SaaS (B2B):**
- €49-199/Monat pro Einrichtung
- Schneller Time-to-Market
- Internationale Expansion

**On-Premise Enterprise (B2B):**  
- €25.000-75.000 Setup + €5.000/Jahr
- Government/High-Security Clients
- Höhere Margen

**Hybrid Migration Services:**
- Cloud → On-Premise: €10.000
- On-Premise → Cloud: €5.000
- Recurring Revenue Opportunities