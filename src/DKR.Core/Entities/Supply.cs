using DKR.Shared.Enums;

namespace DKR.Core.Entities;

public class Supply
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SessionId { get; set; } = string.Empty;
    public bool SterileSyringe { get; set; }
    public bool SterileNeedle { get; set; }
    public bool Filter { get; set; }
    public bool Spoon { get; set; }
    public bool AlcoholSwab { get; set; }
    public bool Tourniquet { get; set; }
    public Session Session { get; set; } = null!;
    public Supply() { }
    public Supply(string sessionId, bool sterileSyringe, bool sterileNeedle, bool filter, bool spoon, bool alcoholSwab, bool tourniquet)
    {
        SessionId = sessionId;
        SterileSyringe = sterileSyringe;
        SterileNeedle = sterileNeedle;
        Filter = filter;
        Spoon = spoon;
        AlcoholSwab = alcoholSwab;
        Tourniquet = tourniquet;
    }
}