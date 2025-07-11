# DKR System - Automatisches Setup Script
# Führe dieses Script als Administrator aus

Write-Host "🏥 DKR System Setup wird gestartet..." -ForegroundColor Green

# Überprüfe Administrator-Rechte
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "❌ Fehler: Dieses Script muss als Administrator ausgeführt werden!" -ForegroundColor Red
    Write-Host "Rechtsklick auf PowerShell → 'Als Administrator ausführen'" -ForegroundColor Yellow
    pause
    exit 1
}

Write-Host "✅ Administrator-Rechte verfügbar" -ForegroundColor Green

# Funktion zum Überprüfen ob Befehl existiert
function Test-Command {
    param($cmdname)
    try {
        if (Get-Command $cmdname -ErrorAction Stop) { return $true }
    }
    catch { return $false }
}

# .NET 8 SDK Installation überprüfen
Write-Host "🔍 Überprüfe .NET 8 SDK..." -ForegroundColor Yellow

if (Test-Command "dotnet") {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK gefunden: $dotnetVersion" -ForegroundColor Green
    
    # Überprüfe ob .NET 8 verfügbar ist
    $net8Available = dotnet --list-sdks | Select-String "8\."
    if (-not $net8Available) {
        Write-Host "⚠️ .NET 8 SDK nicht gefunden. Installiere .NET 8..." -ForegroundColor Yellow
        
        # Download und Installation .NET 8 SDK
        $downloadUrl = "https://download.microsoft.com/download/8/4/8/848f28ae-78a0-4663-96b5-5c1fb1353de1/dotnet-sdk-8.0.100-win-x64.exe"
        $installerPath = "$env:TEMP\dotnet-sdk-8.0.100-win-x64.exe"
        
        try {
            Invoke-WebRequest -Uri $downloadUrl -OutFile $installerPath
            Start-Process -FilePath $installerPath -ArgumentList "/quiet" -Wait
            Write-Host "✅ .NET 8 SDK erfolgreich installiert" -ForegroundColor Green
        }
        catch {
            Write-Host "❌ Fehler beim Download/Installation der .NET 8 SDK" -ForegroundColor Red
            Write-Host "Bitte manuell von https://dotnet.microsoft.com/download installieren" -ForegroundColor Yellow
            pause
            exit 1
        }
    }
} else {
    Write-Host "❌ .NET SDK nicht gefunden. Installiere .NET 8..." -ForegroundColor Red
    
    # Download und Installation .NET 8 SDK
    $downloadUrl = "https://download.microsoft.com/download/8/4/8/848f28ae-78a0-4663-96b5-5c1fb1353de1/dotnet-sdk-8.0.100-win-x64.exe"
    $installerPath = "$env:TEMP\dotnet-sdk-8.0.100-win-x64.exe"
    
    try {
        Write-Host "📥 Lade .NET 8 SDK herunter..." -ForegroundColor Yellow
        Invoke-WebRequest -Uri $downloadUrl -OutFile $installerPath
        Write-Host "🔧 Installiere .NET 8 SDK..." -ForegroundColor Yellow
        Start-Process -FilePath $installerPath -ArgumentList "/quiet" -Wait
        Write-Host "✅ .NET 8 SDK erfolgreich installiert" -ForegroundColor Green
        
        # PATH aktualisieren
        $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH","User")
    }
    catch {
        Write-Host "❌ Fehler beim Download/Installation der .NET 8 SDK" -ForegroundColor Red
        Write-Host "Bitte manuell von https://dotnet.microsoft.com/download installieren" -ForegroundColor Yellow
        pause
        exit 1
    }
}

# SQL Server LocalDB überprüfen
Write-Host "🔍 Überprüfe SQL Server LocalDB..." -ForegroundColor Yellow

$localDbPath = "${env:ProgramFiles}\Microsoft SQL Server\150\Tools\Binn\SqlLocalDB.exe"
if (Test-Path $localDbPath) {
    Write-Host "✅ SQL Server LocalDB gefunden" -ForegroundColor Green
} else {
    Write-Host "⚠️ SQL Server LocalDB nicht gefunden" -ForegroundColor Yellow
    Write-Host "📥 Lade SQL Server Express LocalDB herunter..." -ForegroundColor Yellow
    
    $sqlExpressUrl = "https://download.microsoft.com/download/7/c/1/7c14e92e-bdcb-4f89-b7cf-93543e7112d1/SqlLocalDB.msi"
    $sqlInstallerPath = "$env:TEMP\SqlLocalDB.msi"
    
    try {
        Invoke-WebRequest -Uri $sqlExpressUrl -OutFile $sqlInstallerPath
        Write-Host "🔧 Installiere SQL Server LocalDB..." -ForegroundColor Yellow
        Start-Process -FilePath "msiexec.exe" -ArgumentList "/i `"$sqlInstallerPath`" /quiet /norestart" -Wait
        Write-Host "✅ SQL Server LocalDB erfolgreich installiert" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Fehler beim Download/Installation von SQL Server LocalDB" -ForegroundColor Red
        Write-Host "Bitte manuell von https://www.microsoft.com/en-us/sql-server/sql-server-downloads installieren" -ForegroundColor Yellow
        pause
        exit 1
    }
}

# Zum Projektverzeichnis wechseln
$projectPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Split-Path -Parent $projectPath  # Ein Verzeichnis höher (aus scripts/ raus)
Set-Location $projectPath

Write-Host "📁 Projektverzeichnis: $projectPath" -ForegroundColor Cyan

# Überprüfe ob DKRSystem.sln existiert
if (-not (Test-Path "DKRSystem.sln")) {
    Write-Host "❌ Fehler: DKRSystem.sln nicht gefunden!" -ForegroundColor Red
    Write-Host "Stelle sicher, dass du im korrekten DKR Projektverzeichnis bist." -ForegroundColor Yellow
    pause
    exit 1
}

Write-Host "✅ DKR Solution gefunden" -ForegroundColor Green

# NuGet Packages wiederherstellen
Write-Host "📦 Lade NuGet Packages..." -ForegroundColor Yellow
try {
    dotnet restore
    Write-Host "✅ NuGet Packages erfolgreich geladen" -ForegroundColor Green
}
catch {
    Write-Host "❌ Fehler beim Laden der NuGet Packages" -ForegroundColor Red
    Write-Host "Fehler: $($_.Exception.Message)" -ForegroundColor Red
    pause
    exit 1
}

# Solution bauen
Write-Host "🔨 Baue DKR Solution..." -ForegroundColor Yellow
try {
    dotnet build --configuration Release
    Write-Host "✅ Solution erfolgreich gebaut" -ForegroundColor Green
}
catch {
    Write-Host "❌ Fehler beim Bauen der Solution" -ForegroundColor Red
    Write-Host "Fehler: $($_.Exception.Message)" -ForegroundColor Red
    pause
    exit 1
}

# Datenbank initialisieren
Write-Host "🗄️ Initialisiere Datenbank..." -ForegroundColor Yellow
try {
    # LocalDB Instanz starten
    & "C:\Program Files\Microsoft SQL Server\150\Tools\Binn\SqlLocalDB.exe" create "MSSQLLocalDB" -s
    
    # Entity Framework Migrations ausführen (falls vorhanden)
    if (Test-Path "src/DKR.Infrastructure/Migrations") {
        dotnet ef database update --project src/DKR.Infrastructure --startup-project src/DKR.Web
    }
    
    Write-Host "✅ Datenbank erfolgreich initialisiert" -ForegroundColor Green
}
catch {
    Write-Host "⚠️ Warnung: Datenbank-Initialisierung übersprungen" -ForegroundColor Yellow
    Write-Host "Die Datenbank wird beim ersten Start automatisch erstellt." -ForegroundColor Cyan
}

# Firewall-Regel für HTTPS (Port 7001)
Write-Host "🔒 Konfiguriere Firewall für HTTPS..." -ForegroundColor Yellow
try {
    $firewallRule = Get-NetFirewallRule -DisplayName "DKR System HTTPS" -ErrorAction SilentlyContinue
    if (-not $firewallRule) {
        New-NetFirewallRule -DisplayName "DKR System HTTPS" -Direction Inbound -Protocol TCP -LocalPort 7001 -Action Allow
        Write-Host "✅ Firewall-Regel für Port 7001 erstellt" -ForegroundColor Green
    } else {
        Write-Host "✅ Firewall-Regel bereits vorhanden" -ForegroundColor Green
    }
}
catch {
    Write-Host "⚠️ Warnung: Firewall-Regel konnte nicht erstellt werden" -ForegroundColor Yellow
    Write-Host "Port 7001 manuell in der Windows Firewall freigeben." -ForegroundColor Cyan
}

# SSL-Zertifikat für Development
Write-Host "🔐 Konfiguriere SSL-Zertifikat..." -ForegroundColor Yellow
try {
    dotnet dev-certs https --clean
    dotnet dev-certs https --trust
    Write-Host "✅ SSL-Zertifikat erfolgreich konfiguriert" -ForegroundColor Green
}
catch {
    Write-Host "⚠️ Warnung: SSL-Zertifikat-Konfiguration fehlgeschlagen" -ForegroundColor Yellow
    Write-Host "Das System startet trotzdem, aber HTTPS könnte Warnungen anzeigen." -ForegroundColor Cyan
}

# Desktop-Shortcut erstellen
Write-Host "🖥️ Erstelle Desktop-Shortcut..." -ForegroundColor Yellow
try {
    $desktopPath = [Environment]::GetFolderPath("Desktop")
    $shortcutPath = "$desktopPath\DKR System.lnk"
    
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = "powershell.exe"
    $shortcut.Arguments = "-NoExit -Command `"cd '$projectPath'; dotnet run --project src/DKR.Web`""
    $shortcut.WorkingDirectory = $projectPath
    $shortcut.Description = "DKR System starten"
    $shortcut.Save()
    
    Write-Host "✅ Desktop-Shortcut erstellt: $shortcutPath" -ForegroundColor Green
}
catch {
    Write-Host "⚠️ Warnung: Desktop-Shortcut konnte nicht erstellt werden" -ForegroundColor Yellow
}

# Abschluss
Write-Host ""
Write-Host "🎉 DKR System Setup erfolgreich abgeschlossen!" -ForegroundColor Green
Write-Host ""
Write-Host "🚀 NÄCHSTE SCHRITTE:" -ForegroundColor Cyan
Write-Host "1. DKR System starten:" -ForegroundColor White
Write-Host "   dotnet run --project src/DKR.Web" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Browser öffnen:" -ForegroundColor White
Write-Host "   https://localhost:7001" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Oder Desktop-Shortcut verwenden:" -ForegroundColor White
Write-Host "   'DKR System' auf dem Desktop doppelklicken" -ForegroundColor Gray
Write-Host ""
Write-Host "📚 Weitere Informationen:" -ForegroundColor Cyan
Write-Host "   - DEPLOYMENT_GUIDE.md für Details" -ForegroundColor Gray
Write-Host "   - CLAUDE.md für Entwickler-Hinweise" -ForegroundColor Gray
Write-Host ""

# Option: Direkt starten
$startNow = Read-Host "Möchten Sie das DKR System jetzt starten? (j/n)"
if ($startNow -eq "j" -or $startNow -eq "J" -or $startNow -eq "y" -or $startNow -eq "Y") {
    Write-Host ""
    Write-Host "🚀 Starte DKR System..." -ForegroundColor Green
    Write-Host "Browser wird automatisch geöffnet: https://localhost:7001" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Zum Beenden: Strg+C drücken" -ForegroundColor Yellow
    Write-Host ""
    
    # Browser nach 3 Sekunden öffnen
    Start-Job -ScriptBlock {
        Start-Sleep 3
        Start-Process "https://localhost:7001"
    }
    
    # DKR System starten
    dotnet run --project src/DKR.Web
} else {
    Write-Host ""
    Write-Host "Setup abgeschlossen. Starten Sie das System mit:" -ForegroundColor Green
    Write-Host "dotnet run --project src/DKR.Web" -ForegroundColor Cyan
    Write-Host ""
    pause
}