using Ecommerce.SharedLibrary.Domain;

namespace Ecommerce.SharedLibrary.Events
{
    /// <summary>
    /// Published when a product is successfully created.
    /// Inventory API consumes this event to initialize stock tracking.
    /// </summary>
    public sealed record ProductCreatedEvent(
       Guid ProductId,
       string Sku,
       string ProductName) : BaseEvent;
}