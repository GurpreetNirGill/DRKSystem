# DKR Management System

Ein Blazor-basiertes System für Drug Consumption Room (DKR) Management in Deutschland/Europa.

## 🚀 Quick Start für Entwickler

### 1. Projekt builden und starten

```bash
# Dependencies installieren
dotnet restore

# Projekt builden
dotnet build

# Mit Docker starten (empfohlen)
docker-compose up -d

# ODER direkt starten
dotnet run --project src/DKR.Web
```

Die Anwendung läuft dann unter: http://localhost:5000

### 2. Geschätzter Arbeitsaufwand

**Gesamt: 18-26 Stunden**

- Core Features implementieren: 8-12 Stunden
- UI/UX fertigstellen: 4-6 Stunden  
- Testing & Dokumentation: 3-4 Stunden
- Deployment vorbereiten: 3-4 Stunden

## 📁 Projektstruktur

```
src/
├── DKR.Web/                  # Blazor UI (Hauptprojekt)
├── DKR.Core/                 # Business Logic & Entities
├── DKR.Infrastructure/       # Datenbank & Repositories
├── DKR.Infrastructure.Cloud/ # Cloud Services (Azure/AWS)
├── DKR.Infrastructure.OnPremise/ # Lokale Services
└── DKR.Shared/              # Gemeinsame DTOs & Enums
```

## ⚠️ Aktueller Status

Das Projekt kompiliert erfolgreich! Alle kritischen Build-Fehler wurden behoben.

### Was bereits funktioniert:
✅ Komplette Projektstruktur  
✅ Alle Domain Models und Entities  
✅ Service Interfaces und Basis-Implementierungen  
✅ Docker Support  
✅ Basis UI-Komponenten  

### Was noch implementiert werden muss:
- Client Check-In/Check-Out Flow vervollständigen
- Session Management (30-Minuten Timer)
- Emergency Response System aktivieren
- Inventory Management UI
- Datenbank-Seeding für Testdaten

## 🛠️ Wichtige Features

- **Client-UUID Format**: `KL-YYYY-NNNN` (z.B. KL-2024-0001)
- **Session-Limit**: Max. 30 Minuten pro Client
- **Sprachen**: 9 Sprachen unterstützt
- **Deployment**: Cloud + On-Premise Hybrid-Architektur
- **Compliance**: GDPR, KDS 3.0, TDI 3.0

## 📚 Weitere Dokumentation

- **dkr_blazor_todo.md** - Originalspezifikation mit allen Features
- **dkr_clickdummy.html** - Interaktiver UI-Prototyp (im Browser öffnen!)
- **docs/** - Ordner mit Setup-Guides, Fix-Dokumentation und Übergabedetails

## 🔧 Entwicklungshinweise

1. Die Anwendung nutzt Blazor Server mit SignalR für Echtzeit-Updates
2. Entity Framework Core mit Code-First Ansatz
3. Repository Pattern für Datenzugriff
4. Hybrid-Architektur unterstützt sowohl Cloud als auch On-Premise

## ❓ Bei Problemen

1. Check die **docs/fixes/** für bereits gelöste Probleme
2. Nutze den UI-Prototyp (dkr_clickdummy.html) um den Workflow zu verstehen
3. Die Originalspezifikation (dkr_blazor_todo.md) enthält alle Business Requirements

---

**Viel Erfolg mit dem Projekt! Die Basis ist solide - es fehlt nur noch die Implementierung der Core Features.**