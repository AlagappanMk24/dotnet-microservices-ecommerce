using Ecommerce.SharedLibrary.Domain;

namespace InventoryApi.Domain.Entities;

public sealed class ProcessedOrderEvent : BaseEntity
{
    private ProcessedOrderEvent() { }

    public Guid OrderId { get; private set; }
    public bool Succeeded { get; private set; }
    public string? Message { get; private set; }

    public static ProcessedOrderEvent Create(Guid orderId, bool succeeded, string? message) =>
        new()
        {
            OrderId = orderId,
            Succeeded = succeeded,
            Message = message
        };
}