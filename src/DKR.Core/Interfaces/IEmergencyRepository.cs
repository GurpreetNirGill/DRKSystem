using DKR.Core.Entities;

namespace DKR.Core.Interfaces;

public interface IEmergencyRepository
{
    Task<EmergencyEvent> CreateAsync(EmergencyEvent emergencyEvent);
    Task<EmergencyEvent?> GetByIdAsync(string id);
    Task<IEnumerable<EmergencyEvent>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<EmergencyEvent>> GetByClientIdAsync(string clientId);
    Task<EmergencyEvent> UpdateAsync(EmergencyEvent emergencyEvent);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<EmergencyEvent>> GetAllEmergencyEventsAsync();
}