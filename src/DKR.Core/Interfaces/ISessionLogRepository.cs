using DKR.Core.Entities;
using DKR.Shared.DTOs;

namespace DKR.Core.Interfaces;

public interface ISessionLogRepository
{
    Task<SessionLog> CreateAsync(SessionLog supply);
    Task<SessionLog?> GetByIdAsync(string id);
    Task<IEnumerable<SessionLog>> GetBySessionIdAsync(string clientId);
    Task<SessionLog> UpdateAsync(SessionLog supply);
    Task<bool> DeleteAsync(string id);
    Task<SessionLog?> GetLastSessionLogBySessionIdAsync(string sessionId);
     Task<List<SessionDurationDto>> GetSessionDurationsBySessionIdsAsync(List<string> sessionIds);
}