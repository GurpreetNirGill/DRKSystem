using DKR.Core.Entities;

namespace DKR.Core.Interfaces;

public interface ISupplyRepository
{
    Task<Supply> CreateAsync(Supply supply);
    Task<Supply?> GetByIdAsync(string id);
    Task<IEnumerable<Supply>> GetBySessionIdAsync(string clientId);
    Task<Supply> UpdateAsync(Supply supply);
    Task<bool> DeleteAsync(string id);
}