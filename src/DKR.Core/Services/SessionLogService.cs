using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Shared.DTOs;
using DKR.Shared.Enums;

namespace DKR.Core.Services;

public class SessionLogService
{
    private readonly ISessionLogRepository _sessionLogRepository;
    private readonly INotificationService _notificationService;

    public SessionLogService(ISessionLogRepository sessionLogRepository, INotificationService notificationService)
    {
        _sessionLogRepository = sessionLogRepository;
        _notificationService = notificationService;
    }

    public async Task<SessionLog> CreateSessionLogAsync(string sessionId, SessionStatus status)
    {
        var sessionLog = new SessionLog
        {
            SessionId = sessionId,
            InitialStatus = status,
            StartTime = DateTime.Now,
        };
        var createdSessionLog = await _sessionLogRepository.CreateAsync(sessionLog);

        await _notificationService.NotifyAsync("Erfolg",
            $"Neues Sitzungsprotokoll erstellt: {createdSessionLog.Id}",
            NotificationType.Success);

        return createdSessionLog;
    }

    public async Task<SessionLog> UpdateSessionLogAsync(string sessionId, SessionStatus status)
    {
        var entitySessionLog = await _sessionLogRepository.GetLastSessionLogBySessionIdAsync(sessionId);
        if (entitySessionLog == null)
            throw new ArgumentException("Sitzungsprotokoll nicht gefunden");
            entitySessionLog.EndTime = DateTime.Now;
            entitySessionLog.FinalStatus = status;
            var createdSession = await _sessionLogRepository.UpdateAsync(entitySessionLog);

        await _notificationService.NotifyAsync("Erfolg",
            $"Update-Sitzungsprotokoll erstellt: {entitySessionLog.Id}",
            NotificationType.Success);

        return entitySessionLog;
    }

    public async Task<List<SessionDurationDto>> GetSessionDurationsBySessionIdsAsync(List<string> sessionIds)
    {
        return  await _sessionLogRepository.GetSessionDurationsBySessionIdsAsync(sessionIds);
    }

}

