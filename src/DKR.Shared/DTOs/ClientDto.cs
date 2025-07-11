namespace DKR.Shared.DTOs;

public class ClientDto
{
    public string Id { get; set; } = string.Empty;
    public string UUID { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public string Gender { get; set; } = string.Empty;
    public int BirthYear { get; set; }
    public string? PostalCode { get; set; }
    public string MainSubstance { get; set; } = string.Empty;
    public bool FirstVisit { get; set; }
    public DateTime FirstVisitDate { get; set; }
    public DateTime LastVisitDate { get; set; }
    public string TreatmentHistory { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsArchived { get; set; }
    public string? Nationality { get; set; }
    public int Age { get; set; }
}

public class ClientCreateDto
{
    public string Gender { get; set; } = string.Empty;
    public int BirthYear { get; set; }
    public string? PostalCode { get; set; }
    public string MainSubstance { get; set; } = string.Empty;
    public string TreatmentHistory { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public bool FirstVisit { get; set; } = true;
}

public class ClientCheckInDto
{
    public string ClientId { get; set; } = string.Empty;
    public string MainSubstance { get; set; } = string.Empty;
    public int RoomNumber { get; set; }
}