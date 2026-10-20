using InventoryApi.Application.Abstractions;
using InventoryApi.Application.Contracts;
using InventoryApi.Domain.Entities;
using InventoryApi.Domain.Enums;

namespace InventoryApi.Application.Services;
public sealed class InventoryService(IInventoryRepository repository) : IInventoryService
{
    public async Task<InventoryDto> CreateAsync(CreateInventoryRequest request, CancellationToken ct)
    {
        if (await repository.GetByProductIdAsync(request.ProductId, ct) is not null)
            throw new InvalidOperationException($"Inventory already exists for product '{request.ProductId}'.");

        var item = InventoryItem.Create(
            request.ProductId,
            request.Sku,
            request.ProductName,
            request.InitialQuantity,
            request.ReorderLevel,
            request.WarehouseCode);

        await repository.AddAsync(item, ct);
        await repository.SaveChangesAsync(ct);

        return Map(item);
    }

    public async Task<InventoryDto?> GetByProductIdAsync(Guid productId, CancellationToken ct)
    {
        var item = await repository.GetByProductIdAsync(productId, ct);
        return item is null ? null : Map(item);
    }

    public async Task<IReadOnlyList<InventoryDto>> GetAllAsync(CancellationToken ct) =>
        (await repository.GetAllAsync(ct)).Select(Map).ToArray();

    public async Task<InventoryDto> AddStockAsync(Guid productId, int quantity, CancellationToken ct)
    {
        var item = await repository.GetByProductIdAsync(productId, ct)
            ?? throw new KeyNotFoundException($"Inventory for product '{productId}' was not found.");

        item.AddStock(quantity);
        await repository.SaveChangesAsync(ct);

        return Map(item);
    }

    public async Task<IReadOnlyList<ReservationDto>> GetReservationsAsync(Guid orderId, CancellationToken ct) =>
        (await repository.GetReservationsByOrderIdAsync(orderId, ct))
            .Select(x => new ReservationDto(
                x.Id,
                x.OrderId,
                x.ProductId,
                x.Quantity,
                x.Status.ToString(),
                x.FailureReason))
            .ToArray();

    public async Task ProcessOrderPlacedAsync(OrderPlacedMessage message, CancellationToken ct)
    {
        if (message.OrderId == Guid.Empty || message.Items is null || message.Items.Count == 0)
            throw new ArgumentException("Order event must contain an order ID and at least one item.");

        if (await repository.HasProcessedOrderAsync(message.OrderId, ct))
            return;

        // Each item is validated before mutating any stock, avoiding partial reservations.
        var items = new List<(InventoryItem Inventory, int Quantity)>();

        foreach (var line in message.Items)
        {
            if (line.ProductId == Guid.Empty || line.Quantity <= 0)
                throw new ArgumentException("Order event contains an invalid product or quantity.");

            var inventory = await repository.GetByProductIdAsync(line.ProductId, ct)
                ?? throw new KeyNotFoundException($"Inventory for product '{line.ProductId}' was not found.");

            if (inventory.AvailableQuantity < line.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product '{line.ProductId}'.");

            items.Add((inventory, line.Quantity));
        }

        foreach (var (inventory, quantity) in items)
        {
            inventory.Reserve(quantity);
            await repository.AddReservationAsync(InventoryReservation.Create(message.OrderId, inventory.ProductId, quantity), ct);
        }

        await repository.AddProcessedOrderAsync(ProcessedOrderEvent.Create(message.OrderId, true, "Inventory reserved."), ct);
        await repository.SaveChangesAsync(ct);
    }

    public async Task ReleaseOrderReservationsAsync(Guid orderId, CancellationToken ct)
    {
        var reservations = await repository.GetReservationsByOrderIdAsync(orderId, ct);

        foreach (var reservation in reservations.Where(x => x.Status == ReservationStatus.Reserved))
        {
            var inventory = await repository.GetByProductIdAsync(reservation.ProductId, ct)
                ?? throw new KeyNotFoundException($"Inventory for product '{reservation.ProductId}' was not found.");

            inventory.ReleaseReservation(reservation.Quantity);
            reservation.Release();
        }

        await repository.SaveChangesAsync(ct);
    }

    public async Task FulfillOrderReservationsAsync(Guid orderId, CancellationToken ct)
    {
        var reservations = await repository.GetReservationsByOrderIdAsync(orderId, ct);

        foreach (var reservation in reservations.Where(x => x.Status == ReservationStatus.Reserved))
        {
            var inventory = await repository.GetByProductIdAsync(reservation.ProductId, ct)
                ?? throw new KeyNotFoundException($"Inventory for product '{reservation.ProductId}' was not found.");

            inventory.CommitReservation(reservation.Quantity);
            reservation.Fulfill();
        }

        await repository.SaveChangesAsync(ct);
    }
    public async Task<InventoryDto> CreateFromProductAsync(
    CreateInventoryRequest request,
    CancellationToken ct)
    {
        var existing = await repository.GetByProductIdAsync(
            request.ProductId, ct);

        if (existing is not null)
            return Map(existing);

        var item = InventoryItem.Create(
            request.ProductId,
            request.Sku,
            request.ProductName,
            initialQuantity: 0,
            reorderLevel: request.ReorderLevel,
            warehouseCode: request.WarehouseCode);

        await repository.AddAsync(item, ct);
        await repository.SaveChangesAsync(ct);

        return Map(item);
    }
    private static InventoryDto Map(InventoryItem x) => new(
        x.Id,
        x.ProductId,
        x.Sku,
        x.ProductName,
        x.AvailableQuantity,
        x.ReservedQuantity,
        x.TotalQuantity,
        x.ReorderLevel,
        x.WarehouseCode,
        x.AvailableQuantity <= x.ReorderLevel);
}