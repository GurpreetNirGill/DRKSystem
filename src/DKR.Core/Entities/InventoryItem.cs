namespace DKR.Core.Entities;

public class InventoryItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? TenantId { get; set; }
    public string FacilityId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public int ReorderPoint { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Supplier { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public DateTime LastRestocked { get; set; }
    public string StorageLocation { get; set; } = string.Empty;
    public bool RequiresRefrigeration { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Tracking
    public List<StockMovement> StockMovements { get; set; } = new();
}
public class StockMovement
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string InventoryItemId { get; set; } = string.Empty;
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public DateTime MovementDate { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? RelatedSessionId { get; set; }
}

public enum MovementType
{
    Received,
    Dispensed,
    Expired,
    Damaged,
    Adjustment,
    Transfer
}