using DKR.Core.Entities;
using DKR.Shared.Enums;

namespace DKR.Core.Interfaces;

public interface ISessionRepository
{
    Task<Session> GetByIdAsync(string sessionId);
    Task<IEnumerable<Session>> GetActiveSessionsAsync();
    Task<IEnumerable<Session>> GetActiveSessionsByRoomAsync(string room);
    Task<IEnumerable<Session>> GetByClientIdAsync(string clientId);
    Task<Session> CreateAsync(Session session);
    Task<Session> UpdateAsync(Session session);
    Task<bool> DeleteAsync(string sessionId);
    Task<IEnumerable<Session>> GetSessionsByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<Session>> GetActiveStatusSessionsAsync();
    Task<IEnumerable<Session>> GetEmergencySessionsAsync();
    Task<IEnumerable<Session>> GetActiveAndPauseSessionsAsync();
    Task<IEnumerable<Session>> GetPauseSessionsAsync();

}