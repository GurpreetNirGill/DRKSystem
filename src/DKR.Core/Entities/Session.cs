using DKR.Shared.Enums;

namespace DKR.Core.Entities;

public class Session
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ClientId { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public string Room { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;
    public SubstanceType Substance { get; set; }
    public ApplicationMethod ApplicationMethod { get; set; }
    public SessionStatus Status { get; set; }
    public string? BloodPressure { get; set; }
    public int? Pulse { get; set; }
    public string? Notes { get; set; }
    
    // Navigation property
    public virtual Client Client { get; set; } = null!;
    public virtual Supply Supply { get; set; } = null!;
    public virtual ICollection<SessionLog> SessionLogs { get; set; } = new List<SessionLog>();
    public virtual EmergencyEvent EmergencyEvent { get; set; } = null!;
}

// ApplicationMethod und SessionStatus sind jetzt in DKR.Shared.Enums definiert