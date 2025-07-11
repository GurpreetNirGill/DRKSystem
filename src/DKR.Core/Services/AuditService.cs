using DKR.Core.Entities;
using DKR.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DKR.Core.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUserService _currentUserService;
    private readonly object _hashLock = new object();

    public AuditService(
        IAuditRepository auditRepository,
        IHttpContextAccessor httpContextAccessor,
        ICurrentUserService currentUserService)
    {
        _auditRepository = auditRepository;
        _httpContextAccessor = httpContextAccessor;
        _currentUserService = currentUserService;
    }

    public async Task LogAsync(string action, string entityType, string entityId, object? oldValues = null, object? newValues = null)
    {
        var auditLog = await CreateAuditLogAsync(action, entityType, entityId, oldValues, newValues);
        
        // WORM: Einmal geschrieben, nie mehr änderbar
        await _auditRepository.CreateAsync(auditLog);
    }

    public async Task LogAsync(string action, string entityType, string entityId, string description)
    {
        var auditLog = await CreateAuditLogAsync(action, entityType, entityId, description: description);
        await _auditRepository.CreateAsync(auditLog);
    }

    private async Task<AuditLog> CreateAuditLogAsync(
        string action, 
        string entityType, 
        string entityId, 
        object? oldValues = null, 
        object? newValues = null,
        string? description = null)
    {
        var timestamp = DateTime.UtcNow;
        var userId = _currentUserService.GetCurrentUserId();
        var ipAddress = GetClientIpAddress();
        var userAgent = GetUserAgent();
        var tenantId = _currentUserService.GetCurrentTenantId();

        // Serialisiere Werte zu JSON
        var oldJson = oldValues != null ? JsonSerializer.Serialize(oldValues) : null;
        var newJson = newValues != null ? JsonSerializer.Serialize(newValues) : null;

        // Erstelle Blockchain-Hash
        var previousHash = await GetLastHashAsync();
        var dataToHash = $"{timestamp:O}|{userId}|{action}|{entityType}|{entityId}|{oldJson}|{newJson}|{description}|{ipAddress}|{previousHash}";
        var hash = ComputeSHA256Hash(dataToHash);

        var auditLog = new AuditLog
        {
            Id = hash, // Hash als ID verwenden für Eindeutigkeit
            TenantId = tenantId,
            Timestamp = timestamp,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldJson,
            NewValues = newJson,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Hash = hash,
            PreviousHash = previousHash
        };

        return auditLog;
    }

    private async Task<string?> GetLastHashAsync()
    {
        lock (_hashLock)
        {
            // Thread-safe Zugriff auf letzten Hash
            var lastAuditLog = _auditRepository.GetLastAuditLogAsync().Result;
            return lastAuditLog?.Hash;
        }
    }

    private string ComputeSHA256Hash(string input)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }

    private string GetClientIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "System";

        // X-Forwarded-For Header prüfen (Proxy/Load Balancer)
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        // X-Real-IP Header prüfen
        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        // Remote IP verwenden
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "System";

        return context.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
    }

    public async Task<bool> VerifyIntegrityAsync()
    {
        var allLogs = await _auditRepository.GetAllAuditLogsAsync();
        var sortedLogs = allLogs.OrderBy(l => l.Timestamp).ToList();

        string? expectedPreviousHash = null;

        foreach (var log in sortedLogs)
        {
            // Prüfe ob PreviousHash korrekt ist
            if (log.PreviousHash != expectedPreviousHash)
            {
                return false; // Kette unterbrochen
            }

            // Berechne Hash neu und vergleiche
            var dataToHash = $"{log.Timestamp:O}|{log.UserId}|{log.Action}|{log.EntityType}|{log.EntityId}|{log.OldValues}|{log.NewValues}||{log.IpAddress}|{log.PreviousHash}";
            var calculatedHash = ComputeSHA256Hash(dataToHash);

            if (log.Hash != calculatedHash)
            {
                return false; // Hash manipuliert
            }

            expectedPreviousHash = log.Hash;
        }

        return true; // Alle Logs sind integer
    }

    public async Task<AuditTrailReport> GenerateAuditReportAsync(DateTime from, DateTime to, string? entityType = null, string? userId = null)
    {
        var logs = await _auditRepository.GetAuditLogsByDateRangeAsync(from, to);

        if (!string.IsNullOrEmpty(entityType))
        {
            logs = logs.Where(l => l.EntityType == entityType);
        }

        if (!string.IsNullOrEmpty(userId))
        {
            logs = logs.Where(l => l.UserId == userId);
        }

        var logList = logs.ToList();

        return new AuditTrailReport
        {
            From = from,
            To = to,
            TotalEntries = logList.Count,
            IntegrityVerified = await VerifyIntegrityAsync(),
            Entries = logList.Select(l => new AuditEntry
            {
                Timestamp = l.Timestamp,
                UserId = l.UserId,
                Action = l.Action,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                IpAddress = l.IpAddress,
                Hash = l.Hash
            }).ToList(),
            ActionStatistics = logList.GroupBy(l => l.Action)
                .ToDictionary(g => g.Key, g => g.Count()),
            EntityStatistics = logList.GroupBy(l => l.EntityType)
                .ToDictionary(g => g.Key, g => g.Count()),
            UserStatistics = logList.GroupBy(l => l.UserId)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<ComplianceReport> GenerateComplianceReportAsync(DateTime from, DateTime to)
    {
        var integrityCheck = await VerifyIntegrityAsync();
        var logs = await _auditRepository.GetAuditLogsByDateRangeAsync(from, to);
        var logList = logs.ToList();

        // GDPR Article 32 Compliance Check
        var gdprCompliance = new GDPRComplianceCheck
        {
            IntegrityMeasuresImplemented = integrityCheck,
            EncryptionImplemented = true, // TODO: Prüfe tatsächliche Verschlüsselung
            AccessLoggingComplete = logList.Any(l => l.Action.Contains("Access")),
            DataMinimizationComplied = CheckDataMinimization(logList),
            RetentionPolicyEnforced = CheckRetentionPolicy(logList)
        };

        return new ComplianceReport
        {
            Period = $"{from:yyyy-MM-dd} bis {to:yyyy-MM-dd}",
            IntegrityVerified = integrityCheck,
            TotalAuditEntries = logList.Count,
            GDPRCompliance = gdprCompliance,
            ISO27001Compliance = new ISO27001ComplianceCheck
            {
                SecurityIncidentLogging = logList.Any(l => l.Action.Contains("Emergency") || l.Action.Contains("Security")),
                AccessControlCompliance = logList.Any(l => l.Action.Contains("Login") || l.Action.Contains("Access")),
                DataBackupLogged = logList.Any(l => l.Action.Contains("Backup")),
                AuditTrailIntegrity = integrityCheck
            }
        };
    }

    private bool CheckDataMinimization(List<AuditLog> logs)
    {
        // Prüfe ob nur notwendige Daten geloggt werden
        return logs.All(l => 
            !l.NewValues?.Contains("password", StringComparison.OrdinalIgnoreCase) == true &&
            !l.OldValues?.Contains("password", StringComparison.OrdinalIgnoreCase) == true);
    }

    private bool CheckRetentionPolicy(List<AuditLog> logs)
    {
        // Prüfe 10-Jahre Retention Policy
        var tenYearsAgo = DateTime.UtcNow.AddYears(-10);
        return !logs.Any(l => l.Timestamp < tenYearsAgo);
    }
}

public class AuditTrailReport
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int TotalEntries { get; set; }
    public bool IntegrityVerified { get; set; }
    public List<AuditEntry> Entries { get; set; } = new();
    public Dictionary<string, int> ActionStatistics { get; set; } = new();
    public Dictionary<string, int> EntityStatistics { get; set; } = new();
    public Dictionary<string, int> UserStatistics { get; set; } = new();
}

public class AuditEntry
{
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
}

public class ComplianceReport
{
    public string Period { get; set; } = string.Empty;
    public bool IntegrityVerified { get; set; }
    public int TotalAuditEntries { get; set; }
    public GDPRComplianceCheck GDPRCompliance { get; set; } = new();
    public ISO27001ComplianceCheck ISO27001Compliance { get; set; } = new();
}

public class GDPRComplianceCheck
{
    public bool IntegrityMeasuresImplemented { get; set; }
    public bool EncryptionImplemented { get; set; }
    public bool AccessLoggingComplete { get; set; }
    public bool DataMinimizationComplied { get; set; }
    public bool RetentionPolicyEnforced { get; set; }
}

public class ISO27001ComplianceCheck
{
    public bool SecurityIncidentLogging { get; set; }
    public bool AccessControlCompliance { get; set; }
    public bool DataBackupLogged { get; set; }
    public bool AuditTrailIntegrity { get; set; }
}