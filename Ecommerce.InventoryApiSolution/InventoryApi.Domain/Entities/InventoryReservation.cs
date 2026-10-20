using Ecommerce.SharedLibrary.Domain;
using InventoryApi.Domain.Enums;

namespace InventoryApi.Domain.Entities;

public sealed class InventoryReservation : BaseEntity
{
    private InventoryReservation() { }

    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public ReservationStatus Status { get; private set; }
    public string? FailureReason { get; private set; }

    public static InventoryReservation Create(Guid orderId, Guid productId, int quantity)
    {
        if (orderId == Guid.Empty || productId == Guid.Empty)
            throw new ArgumentException("Order and product IDs are required.");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        return new InventoryReservation
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            Status = ReservationStatus.Reserved
        };
    }

    public void Release()
    {
        if (Status != ReservationStatus.Reserved)
            throw new InvalidOperationException("Only active reservations can be released.");

        Status = ReservationStatus.Released;
        MarkUpdated();
    }

    public void Fulfill()
    {
        if (Status != ReservationStatus.Reserved)
            throw new InvalidOperationException("Only active reservations can be fulfilled.");

        Status = ReservationStatus.Fulfilled;
        MarkUpdated();
    }

    public void Fail(string reason)
    {
        Status = ReservationStatus.Rejected;
        FailureReason = reason;
        MarkUpdated();
    }
}