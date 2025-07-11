using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace DKR.Web.Hubs;

[Authorize]
public class DashboardHub : Hub
{
    private readonly ILogger<DashboardHub> _logger;

    public DashboardHub(ILogger<DashboardHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {UserId} joined group {GroupName}", Context.UserIdentifier, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {UserId} left group {GroupName}", Context.UserIdentifier, groupName);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("User {UserId} connected to dashboard hub", Context.UserIdentifier);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User {UserId} disconnected from dashboard hub", Context.UserIdentifier);
        await base.OnDisconnectedAsync(exception);
    }
}

// SignalR Service für Real-time Updates
public class DashboardNotificationService
{
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly ILogger<DashboardNotificationService> _logger;

    public DashboardNotificationService(IHubContext<DashboardHub> hubContext, ILogger<DashboardNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifySessionUpdate(SessionUpdateNotification notification)
    {
        await _hubContext.Clients.All.SendAsync("SessionUpdate", notification);
        _logger.LogDebug("Session update notification sent: {SessionId}", notification.SessionId);
    }

    public async Task NotifyInventoryAlert(InventoryAlertNotification notification)
    {
        await _hubContext.Clients.All.SendAsync("InventoryAlert", notification);
        _logger.LogDebug("Inventory alert notification sent: {ItemName}", notification.ItemName);
    }

    public async Task NotifyEmergency(EmergencyNotification notification)
    {
        await _hubContext.Clients.All.SendAsync("Emergency", notification);
        _logger.LogWarning("Emergency notification sent: {Type} in {Location}", notification.Type, notification.Location);
    }

    public async Task NotifyKPIUpdate(KPIUpdateNotification notification)
    {
        await _hubContext.Clients.All.SendAsync("KPIUpdate", notification);
    }
}

// Notification Models
public class SessionUpdateNotification
{
    public string SessionId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class InventoryAlertNotification
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public string AlertType { get; set; } = string.Empty; // "LowStock", "Expired", "ExpiringSoon"
    public DateTime AlertTime { get; set; }
}

public class EmergencyNotification
{
    public string EmergencyId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public bool NaloxoneAdministered { get; set; }
    public DateTime OccurredAt { get; set; }
}

public class KPIUpdateNotification
{
    public int ActiveSessions { get; set; }
    public int TodaySessions { get; set; }
    public int UniqueClients { get; set; }
    public int EmergenciesToday { get; set; }
    public Dictionary<string, int> RoomOccupancy { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}