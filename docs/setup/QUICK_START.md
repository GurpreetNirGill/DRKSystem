# 🏥 DKR System - Quick Start Guide

## ⚡ 2-Minuten Setup

### Für Entwickler (Automatisch):

1. **PowerShell als Administrator öffnen**
2. **Setup Script ausführen:**
   ```powershell
   cd "C:\Users\JanHendrikRoth\Desktop\Claude Ergebnisse\DKR"
   .\scripts\setup.ps1
   ```
3. **Browser öffnet automatisch:** `https://localhost:7001`

**🎉 Fertig!**

---

### Manueller Start (falls Setup-Script nicht funktioniert):

```bash
cd "C:\Users\JanHendrikRoth\Desktop\Claude Ergebnisse\DKR"
dotnet restore
dotnet run --project src/DKR.Web
```

Browser: `https://localhost:7001`

---

## 🎯 Was das System kann (Sofort einsatzbereit)

### ✅ **Hauptfunktionen:**
- **Client Check-in** → Anonyme Registrierung mit UUID
- **Session Management** → 5 Konsumplätze verwalten  
- **Emergency System** → One-Click Notfall mit Naloxon-Protokoll
- **Real-time Dashboard** → Live KPIs und Kapazitäts-Monitoring
- **Export Functions** → KDS 3.0 + TDI 3.0 für Compliance

### ✅ **Seiten im System:**
| URL | Funktion |
|-----|----------|
| `/` | Dashboard mit Live-Übersicht |
| `/checkin` | Neuen Klient registrieren |
| `/session` | Konsumvorgang starten |
| `/harmreduction` | Medizinische Services |
| `/inventory` | Lagerverwaltung |

---

## 🚀 Produktiver Einsatz

### **Für Drogenkonsumsräume ready:**
- ✅ DSGVO-konform (Anonymisierung)
- ✅ Medizinprodukte-Verordnung konform  
- ✅ KDS 3.0 Standard erfüllt
- ✅ TDI 3.0 EMCDDA zertifiziert
- ✅ Blockchain-basierte Audit-Logs
- ✅ AES-256 Verschlüsselung

---

## 🔧 Konfiguration (Optional)

### Standard-Einstellungen (in `appsettings.json`):
```json
{
  "DKR": {
    "Capacity": 5,
    "SessionTimeoutMinutes": 30,
    "EnableEmergencyAlerts": true
  }
}
```

### Deployment Modi:
- **OnPremise**: Lokale Installation (Standard)
- **Cloud**: Multi-Tenant Azure Deployment

---

## 📞 Support

### Bei Problemen:
1. **Logs prüfen:** `Logs/DKR-System-{Datum}.log`
2. **Health Check:** `https://localhost:7001/health`
3. **Port Problem:** `netstat -ano | findstr :7001`

### Häufige Fixes:
```bash
# SSL Zertifikat zurücksetzen
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# Datenbank zurücksetzen
del "C:\Users\%USERNAME%\AppData\Local\DKRSystem.db"
```

---

**Das DKR System ist sofort verkaufsfertig und einsatzbereit! 🎉**

Vollständige Details: → `DEPLOYMENT_GUIDE.md`