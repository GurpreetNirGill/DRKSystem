using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Shared.Constants;
using DKR.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DKR.Core.Services;

public class SessionService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Timer _sessionMonitorTimer;
    private readonly IEmergencyRepository _emergencyRepository;
    private readonly ISessionLogRepository _sessionLogRepository;
    private readonly INotificationService _notificationService;
    public event Action? UpdateSession;

    public SessionService(IServiceScopeFactory serviceScopeFactory, IEmergencyRepository emergencyRepository, ISessionLogRepository sessionLogRepository, INotificationService notificationService)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _emergencyRepository = emergencyRepository;
        _sessionLogRepository = sessionLogRepository;
        _notificationService = notificationService;
        // Timer für Session-Überwachung (alle 30 Sekunden)
        _sessionMonitorTimer = new Timer(MonitorActiveSessions, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

    }

    public async Task<Session> StartSessionAsync(Session sessionModal)
    {
        var _sessionLog = new SessionLog();
        using var scope = _serviceScopeFactory.CreateScope();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
        var supplyRepository = scope.ServiceProvider.GetRequiredService<ISupplyRepository>();
        // Prüfe ob Raum verfügbar ist
        var activeSessionsInRoom = await sessionRepository.GetActiveSessionsByRoomAsync(sessionModal.Room);
        if (activeSessionsInRoom.Any())
        {
            throw new InvalidOperationException($"Raum {sessionModal.Room} ist bereits belegt");
        }

        // Prüfe maximale Kapazität
        var allActiveSessions = await sessionRepository.GetActiveAndPauseSessionsAsync();
        if (allActiveSessions.Count() >= 5) // Konfigurierbar
        {
            throw new InvalidOperationException("Maximale Kapazität erreicht");
        }
        sessionModal.StartTime = DateTime.UtcNow;
        sessionModal.Status = SessionStatus.Active;
        _sessionLog.StartTime = sessionModal.StartTime;
        _sessionLog.InitialStatus = SessionStatus.Active;
        sessionModal.SessionLogs.Add(_sessionLog);
        var createdSession = await sessionRepository.CreateAsync(sessionModal);

        await _notificationService.NotifyAsync("Session gestartet", $"Session für Klient {sessionModal.ClientId} in {sessionModal.Room} gestartet", NotificationType.Info);
        NotifySessionChanged();
        return createdSession;
    }

    public async Task<Session> EndSessionAsync(string sessionId, string? notes = null)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();

        var session = await sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException("Session nicht gefunden");
        }
        session.EndTime = DateTime.UtcNow;
        session.Status = SessionStatus.Completed;
        session.Notes = notes;
        var updatedSession = await sessionRepository.UpdateAsync(session);
        var entitySessionLog = await _sessionLogRepository.GetLastSessionLogBySessionIdAsync(session.Id);
        if (entitySessionLog != null)
        {
            entitySessionLog.EndTime = session.EndTime;
            entitySessionLog.FinalStatus = session.Status;
            var createdSession = await _sessionLogRepository.UpdateAsync(entitySessionLog);
        }
        await _notificationService.NotifyAsync("Session beendet", $"Session in {session.Room} erfolgreich beendet", NotificationType.Success);
        NotifySessionChanged();
        return updatedSession;
    }


    public async Task<Session> CreateEmergencySessionAsync(Session sessionModal, Entities.EmergencyType emergencyType)
    {
        var _sessionLog = new SessionLog();
        using var scope = _serviceScopeFactory.CreateScope();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
        var supplyRepository = scope.ServiceProvider.GetRequiredService<ISupplyRepository>();
        // Prüfe ob Raum verfügbar ist
        var activeSessionsInRoom = await sessionRepository.GetActiveSessionsByRoomAsync(sessionModal.Room);
        if (activeSessionsInRoom.Any())
        {
            throw new InvalidOperationException($"Raum {sessionModal.Room} ist bereits belegt");
        }

        // Prüfe maximale Kapazität
        var allActiveSessions = await sessionRepository.GetActiveAndPauseSessionsAsync();
        if (allActiveSessions.Count() >= 5) // Konfigurierbar
        {
            throw new InvalidOperationException("Maximale Kapazität erreicht");
        }
        sessionModal.StartTime = DateTime.UtcNow;
        sessionModal.Status = SessionStatus.Active;
        _sessionLog.StartTime = sessionModal.StartTime;
        _sessionLog.InitialStatus = SessionStatus.Emergency;
        sessionModal.SessionLogs.Add(_sessionLog);
        var createdSession = await sessionRepository.CreateAsync(sessionModal);
        // Notfall-Protokoll erstellen
        var emergencyEvent = new EmergencyEvent
        {
            ClientId = sessionModal.ClientId,
            SessionId = createdSession.Id,
            Type = emergencyType,
            OccurredAt = sessionModal.StartTime,
            Room = sessionModal.Room,
            BloodPressure = sessionModal.BloodPressure,
            HeartRate = sessionModal.Pulse
        };
        var savedEmergency = await _emergencyRepository.CreateAsync(emergencyEvent);
        await _notificationService.NotifyAsync(
    "NOTFALL-Session gestartet",
    $"NOTFALL-Session für Klient {sessionModal.ClientId} in Raum {sessionModal.Room} gestartet",
    NotificationType.Danger
);
        return createdSession;
    }
    public async Task<Session> MarkEmergencyAsync(string sessionId, Entities.EmergencyType emergencyType)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();

        var session = await sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException("Session nicht gefunden");
        }

        session.Status = SessionStatus.Active;
        var updatedSession = await sessionRepository.UpdateAsync(session);

        // Notfall-Protokoll erstellen
        var emergencyEvent = new EmergencyEvent
        {
            ClientId = session.ClientId,
            SessionId = sessionId,
            Type = emergencyType,
            OccurredAt = DateTime.UtcNow,
            Room = session.Room,
            BloodPressure = session.BloodPressure,
            HeartRate = session.Pulse
        };
        var savedEmergency = await _emergencyRepository.CreateAsync(emergencyEvent);
        await _notificationService.NotifyAsync(
    "NOTFALL-Session gestartet",
     $"NOTFALL-Session für Klient {session.ClientId} in Raum {session.Room} gestartet",
 NotificationType.Danger
);
        return updatedSession;
    }

    public async Task<IEnumerable<Session>> GetActiveSessionsAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
        return await sessionRepository.GetActiveSessionsAsync();
    }

    public async Task<IEnumerable<Session>> GetPauseSessionsAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
        return await sessionRepository.GetPauseSessionsAsync();
    }
    public async Task<IEnumerable<Session>> GetActiveStatusSessionsAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
        return await sessionRepository.GetActiveStatusSessionsAsync();
    }

    public async Task<IEnumerable<Session>> GetEmergencySessionsAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
        return await sessionRepository.GetEmergencySessionsAsync();
    }

    public async Task<Session> UpdateSessionStatus(string sessionId, SessionStatus status, bool isForFinalStatus = false)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
        var session = await sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException("Session nicht gefunden");
        }
        if (isForFinalStatus)
            await UpdateSessionLogAsync(sessionId, status);
        else
            await CreateSessionLogAsync(sessionId, status);
        session.Status = status;
        var updatedSession = await sessionRepository.UpdateAsync(session);
        await _notificationService.NotifyAsync($"Session {SystemConstants.GetSessionStatusText(session.Status)}", $"Session in {session.Room} erfolgreich {SystemConstants.GetSessionStatusText(session.Status)}", NotificationType.Warning);
        NotifySessionChanged();
        return updatedSession;
    }

    public async Task<Session> GetSessionByIdAsync(string sessionId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
        return await sessionRepository.GetByIdAsync(sessionId);
    }
    private async Task<SessionLog> CreateSessionLogAsync(string sessionId, SessionStatus status)
    {
        var sessionLog = new SessionLog
        {
            SessionId = sessionId,
            InitialStatus = status,
            StartTime = DateTime.UtcNow,
        };
        var createdSessionLog = await _sessionLogRepository.CreateAsync(sessionLog);

        return createdSessionLog;
    }

    private async Task<SessionLog> UpdateSessionLogAsync(string sessionId, SessionStatus status)
    {
        var entitySessionLog = await _sessionLogRepository.GetLastSessionLogBySessionIdAsync(sessionId);
        if (entitySessionLog == null)
            throw new ArgumentException("Sitzungsprotokoll nicht gefunden");
        entitySessionLog.EndTime = DateTime.UtcNow;
        entitySessionLog.FinalStatus = status;
        var createdSession = await _sessionLogRepository.UpdateAsync(entitySessionLog);

        return entitySessionLog;
    }
    private void NotifySessionChanged() => UpdateSession?.Invoke();
    private async void MonitorActiveSessions(object? state)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
            var sessionLogRepository = scope.ServiceProvider.GetRequiredService<ISessionLogRepository>();

            var maxDuration = TimeSpan.FromMinutes(30);
            var activeSessions = await sessionRepository.GetActiveSessionsAsync();
            var pauseSessions = await sessionRepository.GetPauseSessionsAsync();

            List<string> sessionIds = activeSessions.Select(x => x.Id).ToList();
            var sessionLogDurations = await sessionLogRepository.GetSessionDurationsBySessionIdsAsync(sessionIds);

            foreach (var session in activeSessions)
            {
                var sessionLog = sessionLogDurations.Find(x => x.SessionId == session.Id);
                var totalLoggedTime = sessionLog?.TotalDuration ?? TimeSpan.Zero;
                var startTime = sessionLog?.LastStartDate ?? session.StartTime;
                var currentDuration = DateTime.UtcNow - startTime + totalLoggedTime;
                // Warnung bei 25 Minuten
                if (currentDuration >= TimeSpan.FromMinutes(25) && currentDuration < TimeSpan.FromMinutes(26))
                {
                    await _notificationService.NotifyAsync("Zeit-Warnung", $"Session in {session.Room} läuft seit 25 Minuten", NotificationType.Warning);
                }
                // Automatisches Ende bei 30 Minuten
                if (currentDuration >= maxDuration)
                {
                    await EndSessionAsync(session.Id, "Automatisch beendet nach 30 Minuten");
                    await _notificationService.NotifyAsync("Session beendet", $"Session in {session.Room} automatisch beendet (Zeitüberschreitung)", NotificationType.Warning);
                }
            }

            foreach (var session in pauseSessions)
            {
                var sessionLog = sessionLogDurations.Find(x => x.SessionId == session.Id);
                var totalLoggedTime = sessionLog?.TotalDuration ?? TimeSpan.Zero;
                var startTime = sessionLog?.LastStartDate ?? session.StartTime;
                var currentDuration = DateTime.UtcNow - startTime + totalLoggedTime;
                // Warnung bei 120 Minuten
                if (currentDuration >= TimeSpan.FromMinutes(120))
                {
                    await EndSessionAsync(session.Id, "Automatische Beendigung nach 120 Minuten");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in session monitoring: {ex.Message}");
        }
    }
    public async Task<IEnumerable<Session>> GetActiveAndPauseSessionsAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
        return await sessionRepository.GetActiveAndPauseSessionsAsync();
    }
}