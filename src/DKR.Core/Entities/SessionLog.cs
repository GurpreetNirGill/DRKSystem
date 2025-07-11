using DKR.Shared.Enums;

namespace DKR.Core.Entities;

public class SessionLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SessionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public SessionStatus InitialStatus { get; set; }
    public SessionStatus? FinalStatus { get; set; }
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;
    public virtual Session Session { get; set; } = null!;
}

// ApplicationMethod und SessionStatus sind jetzt in DKR.Shared.Enums definiert