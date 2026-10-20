using Ecommerce.SharedLibrary.Domain;

namespace InventoryApi.Domain.Entities;

public sealed class InventoryItem : BaseEntity
{
    private InventoryItem() { }

    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public int AvailableQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int ReorderLevel { get; private set; }
    public string WarehouseCode { get; private set; } = "MAIN";

    public int TotalQuantity => AvailableQuantity + ReservedQuantity;

    public static InventoryItem Create(
        Guid productId,
        string sku,
        string productName,
        int initialQuantity,
        int reorderLevel,
        string warehouseCode)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId is required.", nameof(productId));

        if (string.IsNullOrWhiteSpace(sku) || sku.Length > 64)
            throw new ArgumentException("SKU is required and must be at most 64 characters.", nameof(sku));

        if (string.IsNullOrWhiteSpace(productName) || productName.Length > 200)
            throw new ArgumentException("Product name is required and must be at most 200 characters.", nameof(productName));

        if (initialQuantity < 0 || reorderLevel < 0)
            throw new ArgumentOutOfRangeException(nameof(initialQuantity), "Quantities cannot be negative.");

        if (string.IsNullOrWhiteSpace(warehouseCode) || warehouseCode.Length > 32)
            throw new ArgumentException("Warehouse code is required and must be at most 32 characters.", nameof(warehouseCode));

        return new InventoryItem
        {
            ProductId = productId,
            Sku = sku.Trim(),
            ProductName = productName.Trim(),
            AvailableQuantity = initialQuantity,
            ReservedQuantity = 0,
            ReorderLevel = reorderLevel,
            WarehouseCode = warehouseCode.Trim().ToUpperInvariant()
        };
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        AvailableQuantity = checked(AvailableQuantity + quantity);
        MarkUpdated();
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        if (AvailableQuantity < quantity)
            throw new InvalidOperationException($"Insufficient stock for product {ProductId}.");

        AvailableQuantity -= quantity;
        ReservedQuantity = checked(ReservedQuantity + quantity);
        MarkUpdated();
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0 || ReservedQuantity < quantity)
            throw new InvalidOperationException("Reservation quantity is invalid.");

        ReservedQuantity -= quantity;
        AvailableQuantity = checked(AvailableQuantity + quantity);
        MarkUpdated();
    }

    public void CommitReservation(int quantity)
    {
        if (quantity <= 0 || ReservedQuantity < quantity)
            throw new InvalidOperationException("Reservation quantity is invalid.");

        ReservedQuantity -= quantity;
        MarkUpdated();
    }
}