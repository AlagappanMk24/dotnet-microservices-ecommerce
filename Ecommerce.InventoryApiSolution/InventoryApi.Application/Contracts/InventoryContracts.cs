namespace InventoryApi.Application.Contracts;

public sealed record CreateInventoryRequest(
    Guid ProductId,
    string Sku,
    string ProductName,
    int InitialQuantity,
    int ReorderLevel = 0,
    string WarehouseCode = "MAIN");

public sealed record AdjustStockRequest(
    int Quantity);

public sealed record InventoryDto(
    Guid Id,
    Guid ProductId,
    string Sku,
    string ProductName,
    int AvailableQuantity,
    int ReservedQuantity,
    int TotalQuantity,
    int ReorderLevel,
    string WarehouseCode,
    bool IsLowStock);

public sealed record ReservationDto(
    Guid Id,
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    string Status,
    string? FailureReason);

public sealed record OrderPlacedMessage(
    Guid OrderId,
    Guid CustomerId,
    decimal Total,
    IReadOnlyList<OrderPlacedMessageItem> Items);

public sealed record OrderPlacedMessageItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice);