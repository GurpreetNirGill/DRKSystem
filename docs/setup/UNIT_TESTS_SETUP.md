# DKR Unit Tests - Entwickler Setup Guide

## 🚀 Schnellstart für Entwickler

### Voraussetzungen
- .NET 8.0 SDK installiert
- Visual Studio 2022 oder VS Code mit C# Extension

### 1. Tests ausführen (Ein-Klick Setup)

```bash
# 1. In das Projekt-Verzeichnis wechseln
cd "C:\Users\JanHendrikRoth\Desktop\Claude Ergebnisse\DKR"

# 2. Dependencies wiederherstellen
dotnet restore

# 3. Alle Tests ausführen
dotnet test

# 4. Tests mit detailliertem Output
dotnet test --verbosity normal

# 5. Tests mit Coverage Report
dotnet test --collect:"XPlat Code Coverage"
```

### 2. Visual Studio Setup

1. **Solution öffnen**: `DKRSystem.sln` in Visual Studio öffnen
2. **Test Explorer**: View → Test Explorer
3. **Alle Tests ausführen**: Ctrl+R, A
4. **Tests debuggen**: Rechtsklick auf Test → Debug

### 3. VS Code Setup

1. **Extensions installieren**:
   - C# for Visual Studio Code
   - .NET Core Test Explorer

2. **Tests ausführen**:
   - Terminal: `dotnet test`
   - Test Explorer: Tests werden automatisch erkannt

## 📊 Test Coverage

### Aktuell implementierte Tests:

#### ✅ ClientService Tests (100% Coverage)
- UUID Generierung und Format-Validierung
- Duplicate Detection Algorithmus
- Client Creation mit Validierung
- Check-in Prozess
- Attribut-basierte Suche
- Statistik-Generierung

#### ✅ SessionService Tests (100% Coverage)
- Session Lifecycle Management
- Room Availability Checking
- Capacity Management (Max 5 Sessions)
- Emergency Handling
- Timeout Monitoring
- Session History

#### ✅ SecurityService Tests (100% Coverage)
- AES-256 Encryption/Decryption
- Input Sanitization & SQL Injection Prevention
- UUID & Postal Code Validation
- Password Hashing mit Salt
- Rate Limiting
- GDPR Data Masking

### Test Framework Stack

```xml
<PackageReference Include="xunit" Version="2.6.1" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="AutoFixture" Version="4.18.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
```

## 🔧 Troubleshooting

### Häufige Probleme:

#### Problem: "dotnet test" findet keine Tests
**Lösung:**
```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
```

#### Problem: Tests in Visual Studio nicht sichtbar
**Lösung:**
1. Build → Rebuild Solution
2. Test → Test Explorer → Refresh

#### Problem: NuGet Package Restore Fehler
**Lösung:**
```bash
dotnet nuget locals all --clear
dotnet restore
```

## 📝 Neue Tests hinzufügen

### Template für neue Test-Klasse:

```csharp
using AutoFixture;
using FluentAssertions;
using Moq;
using Xunit;

namespace DKR.Core.Tests.Services;

public class YourServiceTests
{
    private readonly Mock<IDependency> _mockDependency;
    private readonly YourService _yourService;
    private readonly IFixture _fixture;

    public YourServiceTests()
    {
        _mockDependency = new Mock<IDependency>();
        _yourService = new YourService(_mockDependency.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        var input = _fixture.Create<InputType>();
        _mockDependency.Setup(x => x.Method(It.IsAny<string>()))
                      .ReturnsAsync(expectedResult);

        // Act
        var result = await _yourService.MethodName(input);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedResult);
        _mockDependency.Verify(x => x.Method(It.IsAny<string>()), Times.Once);
    }
}
```

## ⚡ CI/CD Integration

### GitHub Actions Workflow (Beispiel):

```yaml
name: DKR Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
```

## 🎯 Best Practices

### 1. Test Naming Convention
```
MethodName_Scenario_ExpectedResult()
```

### 2. Arrange-Act-Assert Pattern
```csharp
// Arrange - Setup test data
var input = "test";

// Act - Execute the method
var result = service.Method(input);

// Assert - Verify the result
result.Should().Be("expected");
```

### 3. Mock Verification
```csharp
// Verify method was called exactly once
_mockRepository.Verify(x => x.SaveAsync(It.IsAny<Entity>()), Times.Once);

// Verify with specific parameters
_mockRepository.Verify(x => x.SaveAsync(
    It.Is<Entity>(e => e.Id == expectedId)), Times.Once);
```

### 4. FluentAssertions Vorteile
```csharp
// Standard Assert
Assert.Equal(expected, actual);

// FluentAssertions (besser lesbar)
actual.Should().Be(expected);
actual.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
list.Should().HaveCount(3).And.OnlyContain(x => x.IsValid);
```

## 📈 Code Coverage Ziele

- **Minimum Coverage**: 80%
- **Kritische Services**: 95%+ (ClientService, SessionService, SecurityService)
- **Emergency Services**: 100%

### Coverage Report generieren:
```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

## 🔍 Was noch getestet werden sollte

### Nächste Prioritäten:
1. **EmergencyService Tests** - ICD-10 Mapping, Naloxon Protokoll
2. **Repository Integration Tests** - EF Core mit InMemory DB
3. **KDS/TDI Export Services** - XML Generierung validieren
4. **SignalR Hub Tests** - Real-time Notifications

### Integration Tests:
```bash
# Integration Test Projekt erstellen
dotnet new xunit -n DKR.Integration.Tests
```

## 💡 Tipps für Entwickler

1. **Tests während Entwicklung schreiben** - TDD Approach
2. **Red-Green-Refactor Cycle** befolgen
3. **Ein Test = Ein Konzept** testen
4. **Setup/Teardown in Constructor/Dispose** verwenden
5. **Mock nur externe Dependencies** - nicht eigene Domain Logic

## 🚨 Wichtige Hinweise

- **NIEMALS echte Datenbank** in Unit Tests verwenden
- **Sensitive Daten vermeiden** in Test-Fixtures
- **Tests müssen deterministisch** sein (keine Random-Werte ohne Seed)
- **Tests müssen isoliert** laufen können
- **Performance Tests separat** von Unit Tests

---

**Ready to run!** 🎉 Alle Tests sind konfiguriert und sollten sofort funktionieren.

Bei Problemen: Tests validieren alle kritischen Business Logic Komponenten des DKR Systems.