using InventoryApi.Application.Abstractions;
using InventoryApi.Domain.Entities;
using InventoryApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Infrastructure.Repositories;

public sealed class InventoryRepository(InventoryDbContext db) : IInventoryRepository
{
    public async Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.InventoryItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default) =>
        await db.InventoryItems.FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);

    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.InventoryItems
            .AsNoTracking()
            .OrderBy(x => x.ProductName)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(InventoryItem entity, CancellationToken cancellationToken = default) =>
        await db.InventoryItems.AddAsync(entity, cancellationToken);

    public void Update(InventoryItem entity) =>
        db.InventoryItems.Update(entity);

    public void Remove(InventoryItem entity)
    {
        entity.MarkDeleted();
        db.InventoryItems.Update(entity);
    }

    public async Task<IReadOnlyList<InventoryReservation>> GetReservationsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        await db.Reservations
            .Where(x => x.OrderId == orderId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> HasProcessedOrderAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        db.ProcessedOrderEvents.AnyAsync(x => x.OrderId == orderId, cancellationToken);

    public async Task AddReservationAsync(InventoryReservation reservation, CancellationToken cancellationToken = default) =>
        await db.Reservations.AddAsync(reservation, cancellationToken);

    public async Task AddProcessedOrderAsync(ProcessedOrderEvent processedOrder, CancellationToken cancellationToken = default) =>
        await db.ProcessedOrderEvents.AddAsync(processedOrder, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}