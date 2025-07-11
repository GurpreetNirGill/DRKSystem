using DKR.Core.Entities;

namespace DKR.Core.Interfaces;

public interface IInventoryRepository
{
    Task<IEnumerable<InventoryItem>> GetAllAsync();
    Task<InventoryItem?> GetByIdAsync(string id);
    Task<InventoryItem> CreateAsync(InventoryItem item);
    Task<InventoryItem> UpdateAsync(InventoryItem item);
    Task<bool> DeleteAsync(string id);
}