using InventoryApi.Application.Contracts;
namespace InventoryApi.Application.Abstractions;
public interface IInventoryService
{
    Task<InventoryDto> CreateAsync(CreateInventoryRequest request, CancellationToken cancellationToken);
    Task<InventoryDto?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
    Task<IReadOnlyList<InventoryDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<InventoryDto> AddStockAsync(Guid productId, int quantity, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReservationDto>> GetReservationsAsync(Guid orderId, CancellationToken cancellationToken);
    Task ProcessOrderPlacedAsync(OrderPlacedMessage message, CancellationToken cancellationToken);
    Task ReleaseOrderReservationsAsync(Guid orderId, CancellationToken cancellationToken);
    Task FulfillOrderReservationsAsync(Guid orderId, CancellationToken cancellationToken);
    Task<InventoryDto> CreateFromProductAsync (CreateInventoryRequest request,CancellationToken cancellationToken);
}
