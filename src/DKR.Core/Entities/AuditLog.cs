namespace DKR.Core.Entities;

public class AuditLog
{
    public string Id { get; set; } = string.Empty; // Hash-basierte ID
    public string? TenantId { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty; // SHA-256 Hash für Integrität
    public string? PreviousHash { get; set; } // Blockchain-artige Verkettung
}