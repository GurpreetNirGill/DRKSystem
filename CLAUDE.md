# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Wichtige Hinweise

**Mit dem Nutzer dieses Repositories immer auf Deutsch kommunizieren!**

## Project Overview

This repository contains planning and design documents for a DKR (Drug Consumption Room) Management System. The project is a comprehensive Blazor-based system designed for managing drug consumption rooms in Germany and Europe, supporting both cloud and on-premise deployments.

## Key Files

- `dkr_blazor_todo.md`: Detailed development roadmap and technical specifications for the Blazor implementation
- `dkr_clickdummy.html`: Interactive HTML prototype demonstrating the user interface and workflow

## Architecture

The system follows a hybrid cloud/on-premise architecture with these core components:

1. **DKR.Web** - Blazor Server UI (deployment-agnostic)
2. **DKR.Core** - Domain models and business logic
3. **DKR.Infrastructure** - Abstraction layer for storage/identity
4. **DKR.Infrastructure.Cloud** - Azure/AWS service implementations
5. **DKR.Infrastructure.OnPremise** - Local service implementations
6. **DKR.Shared** - DTOs, enums, and constants
7. **DKR.Deployment** - Docker, Helm, and deployment artifacts

## Common Development Commands

### Project Creation
```bash
dotnet new blazorserver -n DKR.Web
dotnet new classlib -n DKR.Core
dotnet new classlib -n DKR.Infrastructure
dotnet new sln -n DKRSystem
dotnet sln add **/*.csproj
```

### Build and Run
```bash
dotnet build
dotnet run --project DKR.Web
```

### Testing
```bash
dotnet test
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
```

## Core Features

The system implements:
- Client check-in/check-out with UUID generation (KL-YYYY-NNNN format)
- Session management with real-time monitoring (30-minute maximum)
- Emergency response system with automated notifications
- Harm reduction services (HIV/HCV testing, wound care)
- Inventory management with FIFO/FEFO logic
- KDS 3.0 and TDI 3.0 compliance for German/EU reporting
- Multi-language support (9 languages)
- Offline-first mobile architecture
- FHIR R4 and HL7 integration for healthcare interoperability

## Deployment Modes

The system supports three deployment modes:
1. **Cloud**: Multi-tenant SaaS with Azure/AWS support
2. **On-Premise**: Single-tenant enterprise deployment
3. **Hybrid**: Migration path between cloud and on-premise

Configuration is managed through environment detection and feature toggles per deployment mode.

## Security and Compliance

- GDPR-compliant with 10-year data retention
- Field-level encryption (AES-256)
- Role-based access control
- Audit trail with WORM (Write Once, Read Many) logging
- Penetration testing integration points
- 2FA support for all deployment modes