# 5-Minuten-Setup für Entwickler - Best Practice Beispiel

## Übersicht

Dieses Dokument erklärt, wie das DKR-Projekt ein "5-Minuten-Setup" für neue Entwickler realisiert hat. Diese Praktiken können für andere .NET-Projekte übernommen werden.

## Das Problem

Neue Entwickler brauchen oft Stunden oder Tage, um ein Projekt lokal zum Laufen zu bringen:
- Fehlende Dependencies
- Unklare Dokumentation
- Manuelle Konfigurationsschritte
- Verschiedene Entwicklungsumgebungen

## Die Lösung: Automatisiertes Setup-Skript

### 1. Ein einziger Befehl

```powershell
.\scripts\setup.ps1
```

Das ist alles! Kein langes README durchlesen, keine 20 manuelle Schritte.

### 2. Was das Skript automatisch erledigt

#### Voraussetzungen prüfen und installieren
```powershell
# Prüft ob .NET 8 SDK vorhanden ist
if (Test-Command "dotnet") {
    $net8Available = dotnet --list-sdks | Select-String "8\."
    if (-not $net8Available) {
        # Lädt und installiert .NET 8 automatisch
        Invoke-WebRequest -Uri $downloadUrl -OutFile $installerPath
        Start-Process -FilePath $installerPath -ArgumentList "/quiet" -Wait
    }
}
```

#### Datenbank-Setup
```powershell
# SQL Server LocalDB automatisch installieren
if (-not (Test-Path $localDbPath)) {
    Invoke-WebRequest -Uri $sqlExpressUrl -OutFile $sqlInstallerPath
    Start-Process -FilePath "msiexec.exe" -ArgumentList "/i `"$sqlInstallerPath`" /quiet" -Wait
}
```

#### Projekt bauen
```powershell
# Dependencies laden und kompilieren
dotnet restore
dotnet build --configuration Release
```

### 3. Entwicklerfreundliche Features

#### Desktop-Shortcut
```powershell
# Erstellt "DKR System" Shortcut auf dem Desktop
$shortcut.TargetPath = "powershell.exe"
$shortcut.Arguments = "-NoExit -Command `"cd '$projectPath'; dotnet run --project src/DKR.Web`""
```

#### Sofort-Start Option
```powershell
$startNow = Read-Host "Möchten Sie das DKR System jetzt starten? (j/n)"
if ($startNow -eq "j") {
    # Browser öffnet automatisch nach 3 Sekunden
    Start-Job -ScriptBlock {
        Start-Sleep 3
        Start-Process "https://localhost:7001"
    }
    dotnet run --project src/DKR.Web
}
```

### 4. Fehlerbehandlung

Das Skript gibt klare Anweisungen bei Problemen:
```powershell
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "❌ Fehler: Dieses Script muss als Administrator ausgeführt werden!" -ForegroundColor Red
    Write-Host "Rechtsklick auf PowerShell → 'Als Administrator ausführen'" -ForegroundColor Yellow
}
```

## Best Practices für andere Projekte

### 1. Alles automatisieren was möglich ist
- SDK/Runtime Installation
- Package Manager Setup
- Datenbank-Initialisierung
- SSL-Zertifikate
- Firewall-Regeln

### 2. Klare Fortschrittsanzeige
```powershell
Write-Host "🏥 DKR System Setup wird gestartet..." -ForegroundColor Green
Write-Host "✅ Administrator-Rechte verfügbar" -ForegroundColor Green
Write-Host "🔍 Überprüfe .NET 8 SDK..." -ForegroundColor Yellow
Write-Host "📦 Lade NuGet Packages..." -ForegroundColor Yellow
Write-Host "🔨 Baue DKR Solution..." -ForegroundColor Yellow
```

### 3. Robuste Fehlerbehandlung
- Prüfe Voraussetzungen bevor Aktionen ausgeführt werden
- Gib hilfreiche Fehlermeldungen
- Biete manuelle Alternativen an

### 4. Am Ende: Klare nächste Schritte
```powershell
Write-Host "🚀 NÄCHSTE SCHRITTE:" -ForegroundColor Cyan
Write-Host "1. DKR System starten:" -ForegroundColor White
Write-Host "   dotnet run --project src/DKR.Web" -ForegroundColor Gray
Write-Host "2. Browser öffnen:" -ForegroundColor White
Write-Host "   https://localhost:7001" -ForegroundColor Gray
```

## Vorteile

1. **Zeitersparnis**: Von 0 auf lauffähig in 5 Minuten
2. **Weniger Frustration**: Keine manuellen Fehlerquellen
3. **Konsistenz**: Jeder Entwickler hat die gleiche Umgebung
4. **Onboarding**: Neue Teammitglieder sind sofort produktiv

## Anpassung für andere Technologie-Stacks

### Node.js/React Beispiel
```bash
#!/bin/bash
# setup.sh
echo "🚀 Projekt-Setup wird gestartet..."

# Node.js prüfen/installieren
if ! command -v node &> /dev/null; then
    curl -fsSL https://deb.nodesource.com/setup_lts.x | sudo -E bash -
    sudo apt-get install -y nodejs
fi

# Dependencies installieren
npm install

# Umgebungsvariablen kopieren
cp .env.example .env

# Datenbank migrieren
npm run migrate

# Optional: Direkt starten
read -p "Projekt jetzt starten? (j/n) " -n 1 -r
if [[ $REPLY =~ ^[Jj]$ ]]; then
    npm run dev
fi
```

### Python/Django Beispiel
```python
#!/usr/bin/env python3
# setup.py
import os
import subprocess
import sys

print("🐍 Django-Projekt Setup...")

# Virtual Environment erstellen
if not os.path.exists('venv'):
    subprocess.run([sys.executable, '-m', 'venv', 'venv'])

# Dependencies installieren
subprocess.run(['venv/bin/pip', 'install', '-r', 'requirements.txt'])

# Datenbank setup
subprocess.run(['venv/bin/python', 'manage.py', 'migrate'])

# Superuser erstellen
subprocess.run(['venv/bin/python', 'manage.py', 'createsuperuser'])

print("✅ Setup abgeschlossen! Starten mit: venv/bin/python manage.py runserver")
```

## Fazit

Ein gutes Setup-Skript ist eine Investition, die sich vielfach auszahlt:
- Entwickler sind schneller produktiv
- Weniger Support-Anfragen
- Konsistente Entwicklungsumgebungen
- Professioneller erster Eindruck

Das DKR-Projekt zeigt: Mit einem durchdachten Setup-Skript wird aus einer potentiell frustrierenden Erfahrung ein reibungsloser Start in unter 5 Minuten.