namespace DKR.Core.Entities;

public class HarmReduction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ClientId { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public ServiceType Type { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ServiceStatus Status { get; set; }
    public string ProvidedBy { get; set; } = string.Empty;
    public string? Results { get; set; }
    public string? Notes { get; set; }
    public bool ReminderSent { get; set; }
    public DateTime ServiceDate { get; set; }
    public bool IsCompleted { get; set; }
    public string? Result { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual Client Client { get; set; } = null!;
}

public enum ServiceType
{
    HIVTest,
    HCVTest,
    HIVHCVCombiTest,
    WoundCare,
    VeinCounseling,
    SubstitutionCounseling,
    FollowUpCare
}

public enum ServiceStatus
{
    Scheduled,
    Confirmed,
    Completed,
    Cancelled,
    NoShow
}