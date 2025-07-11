namespace DKR.Shared.Enums;

public enum DeploymentMode
{
    Auto,
    Cloud, 
    OnPremise, 
    Hybrid
}

public enum TenantMode
{
    Auto,
    Single,
    Multi
}

public enum SessionStatus
{
    Waiting,
    Active,
    Completed,
    Cancelled,
    Emergency,
    Monitoring,
    Pause
}

public enum EmergencyType
{
    Overdose,
    Unconsciousness,
    Seizure,
    RespiratoryArrest,
    Injury,
    Cardiac,
    Psychiatric,
    PsychiatricEmergency,
    Other
}

public enum NotificationChannel
{
    Email,
    SMS,
    WhatsApp,
    Push,
    System
}

public enum ServiceType
{
    HIVTest,
    HCVTest,
    WoundCare,
    VenousAdvice,
    SubstitutionTreatment,
    NaloxoneDistribution,
    CounselingSession,
    HealthCheckup,
    Other
}

public enum ServiceStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}

public enum ApplicationMethod
{
    Intravenous,
    Inhalation,
    Intranasal,
    Oral
}

public enum TreatmentHistory
{
    None,
    Detoxification,
    Substitution,
    Rehabilitation,
    Aftercare
}

public enum Gender
{
    Male,
    Female,
    Diverse,
    NotSpecified
}

public enum SubstanceType
{
    Heroin,
    Cocaine,
    Amphetamines,
    OtherOpioids,
    Cannabis,
    Other
}