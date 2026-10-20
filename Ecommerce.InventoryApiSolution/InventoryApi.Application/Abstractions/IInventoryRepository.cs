using Ecommerce.SharedLibrary.Repositories;
using InventoryApi.Domain.Entities;
namespace InventoryApi.Application.Abstractions;
public interface IInventoryRepository : IRepository<InventoryItem>
{
    Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken=default);
    Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken=default);
    Task<IReadOnlyList<InventoryReservation>> GetReservationsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken=default);
    Task<bool> HasProcessedOrderAsync(Guid orderId, CancellationToken cancellationToken=default);
    Task AddReservationAsync(InventoryReservation reservation, CancellationToken cancellationToken=default);
    Task AddProcessedOrderAsync(ProcessedOrderEvent processedOrder, CancellationToken cancellationToken=default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken=default);
}
