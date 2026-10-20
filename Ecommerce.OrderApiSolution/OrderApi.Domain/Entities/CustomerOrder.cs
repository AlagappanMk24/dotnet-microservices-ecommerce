using Ecommerce.SharedLibrary.Domain;
using OrderApi.Domain.Enums;

namespace OrderApi.Domain.Entities;

public sealed class CustomerOrder : BaseEntity
{
    private readonly List<OrderItem> _items = [];
    private CustomerOrder() { }

    public Guid CustomerId { get; private set; }
    public string Currency { get; private set; } = "USD";
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal Subtotal { get; private set; }
    public decimal Total { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public static CustomerOrder Create(Guid customerId, string currency, IEnumerable<OrderItemDraft> drafts, string? notes = null)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer identity is required.", nameof(customerId));

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new ArgumentException("Currency must be a 3-letter code.", nameof(currency));

        var items = drafts?.ToList() ?? throw new ArgumentNullException(nameof(drafts));

        if (items.Count == 0)
            throw new ArgumentException("At least one order item is required.", nameof(drafts));

        if (items.Count > 100)
            throw new ArgumentException("An order cannot contain more than 100 lines.", nameof(drafts));

        var order = new CustomerOrder
        {
            CustomerId = customerId,
            Currency = currency.Trim().ToUpperInvariant(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };

        foreach (var item in items)
        {
            order._items.Add(OrderItem.Create(
                item.ProductId,
                item.ProductName,
                item.Sku,
                item.Quantity,
                item.UnitPrice));
        }

        order.Subtotal = decimal.Round(order._items.Sum(x => x.LineTotal), 2, MidpointRounding.AwayFromZero);
        order.Total = order.Subtotal;

        return order;
    }

    public void ChangeStatus(OrderStatus next)
    {
        var allowed = Status switch
        {
            OrderStatus.Pending => next is OrderStatus.Confirmed or OrderStatus.Cancelled or OrderStatus.Failed,
            OrderStatus.Confirmed => next is OrderStatus.Processing or OrderStatus.Cancelled or OrderStatus.Failed,
            OrderStatus.Processing => next is OrderStatus.Shipped or OrderStatus.Cancelled or OrderStatus.Failed,
            OrderStatus.Shipped => next is OrderStatus.Delivered or OrderStatus.Failed,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException($"Cannot change order status from {Status} to {next}.");

        Status = next;
        MarkUpdated();
    }
}

public sealed record OrderItemDraft(
    Guid ProductId,
    string ProductName,
    string Sku,
    int Quantity,
    decimal UnitPrice);