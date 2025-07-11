namespace DKR.Core.Entities;

public class EmergencyEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ClientId { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string? TenantId { get; set; }
    public DateTime OccurredAt { get; set; }
    public EmergencyType Type { get; set; }
    public string Room { get; set; } = string.Empty;
    public List<string> ActionsPerformed { get; set; } = new();
    public bool NaloxoneAdministered { get; set; }
    public bool EmergencyServicesCalled { get; set; }
    public string? Outcome { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    
    // ICD-10 Klassifikation
    public string? ICD10Code { get; set; }
    
    // Vitalzeichen
    public string? VitalSigns { get; set; }
    public string? BloodPressure { get; set; }
    public int? HeartRate { get; set; }
    public string? ConsciousnessLevel { get; set; }
    
    // Naloxon Details
    public int NaloxoneDoses { get; set; }
    public DateTime? NaloxoneAdministeredAt { get; set; }
    
    // Response Tracking
    public DateTime? EmergencyServicesNotifiedAt { get; set; }
    public DateTime? EmergencyServicesArrivedAt { get; set; }
    public DateTime? IncidentResolvedAt { get; set; }
    
    // Follow-up
    public bool RequiresFollowUp { get; set; }
    public string? FollowUpNotes { get; set; }
    public bool ReportedToAuthorities { get; set; }
    public DateTime? AuthorityReportSentAt { get; set; }

    public string Description { get; set; } = string.Empty;

    // Navigation property
    public virtual Client Client { get; set; } = null!;
    public virtual Session Session { get; set; } = null!;
}

public enum EmergencyType
{
    Overdose,
    Unconsciousness,
    Seizure,
    RespiratoryArrest,
    Injury,
    PsychiatricEmergency,
    Cardiac,
    Psychiatric,
    Other
}