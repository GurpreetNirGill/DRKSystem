using DKR.Core.Entities;
using DKR.Core.Interfaces;

namespace DKR.Core.Services;

public class InventoryService
{
    private readonly IInventoryRepository _repository;
    private readonly INotificationService _notificationService;

    public InventoryService(IInventoryRepository repository, INotificationService notificationService)
    {
        _repository = repository;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<InventoryItem>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<InventoryItem?> GetByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<InventoryItem> CreateAsync(InventoryItem item)
    {
        item.Id = Guid.NewGuid().ToString();
        item.CreatedAt = DateTime.UtcNow;
        return await _repository.CreateAsync(item);
    }

    public async Task<InventoryItem> UpdateAsync(InventoryItem item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(item);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Where(i => i.Quantity <= i.MinimumStock);
    }

    public async Task<IEnumerable<InventoryItem>> GetExpiringItemsAsync(int daysBeforeExpiry = 30)
    {
        var items = await _repository.GetAllAsync();
        var cutoffDate = DateTime.UtcNow.AddDays(daysBeforeExpiry);
        return items.Where(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value <= cutoffDate);
    }

    public async Task<InventoryItem> UseItemAsync(string itemId, int quantity)
    {
        var item = await _repository.GetByIdAsync(itemId);
        if (item == null)
            throw new ArgumentException("Item not found");

        if (item.Quantity < quantity)
            throw new InvalidOperationException("Insufficient stock");

        item.Quantity -= quantity;
        item.UpdatedAt = DateTime.UtcNow;

        // Check for low stock
        if (item.Quantity <= item.MinimumStock)
        {
            await _notificationService.SendAsync(new NotificationRequest
            {
                Channel = Shared.Enums.NotificationChannel.Email,
                Subject = "Niedriger Lagerbestand",
                Message = $"{item.Name} hat nur noch {item.Quantity} {item.Unit}. Mindestbestand: {item.MinimumStock}"
            });
        }

        return await _repository.UpdateAsync(item);
    }

    public async Task<InventoryItem> RestockItemAsync(string itemId, int quantity, string? batchNumber = null, DateTime? expiryDate = null)
    {
        var item = await _repository.GetByIdAsync(itemId);
        if (item == null)
            throw new ArgumentException("Item not found");

        item.Quantity += quantity;
        if (!string.IsNullOrEmpty(batchNumber))
            item.BatchNumber = batchNumber;
        if (expiryDate.HasValue)
            item.ExpiryDate = expiryDate;
        item.UpdatedAt = DateTime.UtcNow;

        return await _repository.UpdateAsync(item);
    }

    public async Task<Dictionary<string, int>> GetCategoryStatisticsAsync()
    {
        var items = await _repository.GetAllAsync();
        return items
            .GroupBy(i => i.Category)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
    }

    public async Task<bool> CheckExpiryDatesAsync()
    {
        var expiringItems = await GetExpiringItemsAsync(7); // 7 days warning
        
        foreach (var item in expiringItems)
        {
            await _notificationService.SendAsync(new NotificationRequest
            {
                Channel = Shared.Enums.NotificationChannel.System,
                Subject = "Ablaufdatum Warnung",
                Message = $"{item.Name} läuft ab am {item.ExpiryDate:dd.MM.yyyy}"
            });
        }

        return expiringItems.Any();
    }
}