using DKR.Core.Entities;

namespace DKR.Core.Interfaces;

public interface IHarmReductionRepository
{
    Task<HarmReduction> GetByIdAsync(string id);
    Task<IEnumerable<HarmReduction>> GetAllAsync();
    Task<HarmReduction> CreateAsync(HarmReduction service);
    Task<HarmReduction> UpdateAsync(HarmReduction service);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<HarmReduction>> GetByClientIdAsync(string clientId);
    Task<IEnumerable<HarmReduction>> GetByTypeAsync(ServiceType serviceType);
    Task<IEnumerable<HarmReduction>> GetServicesByDateRangeAsync(DateTime from, DateTime to);
}