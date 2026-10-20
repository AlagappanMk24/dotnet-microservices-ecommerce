using Ecommerce.SharedLibrary.Domain;

namespace Ecommerce.SharedLibrary.Events;

public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal Total,
    IReadOnlyList<OrderPlacedItem> Items)
    : BaseEvent;

public sealed record OrderPlacedItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice);