namespace DKR.Shared.DTOs;

public class SessionDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientUUID { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Room { get; set; }
    public string MainSubstance { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public TimeSpan? RemainingTime { get; set; }
}

public class SessionCreateDto
{
    public string ClientId { get; set; } = string.Empty;
    public int RoomNumber { get; set; }
    public string MainSubstance { get; set; } = string.Empty;
}

public class ActiveSessionDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientUUID { get; set; } = string.Empty;
    public string RoomNumber { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan RemainingTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SessionDurationDto
{
    public string SessionId { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; } 
    public DateTime LastStartDate { get; set; }
}