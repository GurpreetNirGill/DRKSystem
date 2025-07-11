using DKR.Shared.Enums;

namespace DKR.Core.Entities;

public class Client
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UUID { get; set; } = string.Empty; // Format: KL-YYYY-NNNN
    public string? TenantId { get; set; } // Für Cloud Multi-tenancy
    public Gender Gender { get; set; }
    public int BirthYear { get; set; }
    public string? PostalCode { get; set; }
    public SubstanceType MainSubstance { get; set; }
    public bool FirstVisit { get; set; }
    public DateTime FirstVisitDate { get; set; }
    public DateTime LastVisitDate { get; set; }
    public TreatmentHistory TreatmentHistory { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    
    // Additional properties for repository compatibility
    public string? Nationality { get; set; }
    public int Age => DateTime.Now.Year - BirthYear;
    public DateTime? LastCheckIn => LastVisitDate == DateTime.MinValue ? null : LastVisitDate;
    
    // Navigation properties
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<HarmReduction> Services { get; set; } = new List<HarmReduction>();
    public virtual ICollection<EmergencyEvent> EmergencyEvents { get; set; } = new List<EmergencyEvent>();
}

// Enums sind jetzt in DKR.Shared.Enums definiert